#region Copyright 
// Copyright 2017 Gygya Inc.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Hyperscale.Common.Application.HttpService.Client;
using Hyperscale.Common.Contracts.Exceptions;
using Hyperscale.Common.Contracts.HttpService;
using Hyperscale.Microcore.Interfaces.Events;
using Hyperscale.Microcore.Interfaces.Logging;
using Hyperscale.Microcore.ServiceDiscovery;
using Hyperscale.Microcore.ServiceDiscovery.Config;
using Hyperscale.Microcore.SharedLogic;
using Hyperscale.Microcore.SharedLogic.Events;
using Hyperscale.Microcore.SharedLogic.Exceptions;
using Hyperscale.Microcore.SharedLogic.HttpService;
using Hyperscale.Microcore.SharedLogic.Security;
using Newtonsoft.Json;

namespace Hyperscale.Microcore.ServiceProxy
{
    public class ServiceProxyProvider : IDisposable, IServiceProxyProvider
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DateParseHandling = DateParseHandling.None
        };

        /// <summary>
        /// Gets or sets default port used to access the remote service, it overridden by service discovery.
        /// </summary>
        public int? DefaultPort { get; set; }

        /// <summary>
        /// Gets the name of the remote service from the interface name.
        /// is used.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// GetObject a value indicating if a secure will be used to connect to the remote service. This defaults to the
        /// value that was specified in the <see cref="HttpServiceAttribute"/> decorating <i>TInterface</i>, overridden
        /// by service discovery.
        /// </summary>
        public bool UseHttpsDefault { get; set; }


        /// <summary>
        /// Specifies a delegate that can be used to change a request in a user-defined way before it is sent over the
        /// network.
        /// </summary>
        public Action<HttpServiceRequest> PrepareRequest { get; set; }
        public ISourceBlock<string> EndPointsChanged => ServiceDiscovery.EndPointsChanged;
        public ISourceBlock<ServiceReachabilityStatus> ReachabilityChanged => ServiceDiscovery.ReachabilityChanged;
        private TimeSpan? Timeout { get; set; }

        internal IServiceDiscovery ServiceDiscovery { get; set; }

        private HttpMessageHandler _httpMessageHandler = new SocketsHttpHandler();

        protected internal HttpMessageHandler HttpMessageHandler
        {
            get
            {
                lock (HttpClientLock)
                {
                    return _httpMessageHandler;
                }
            }
            set
            {
                lock (HttpClientLock)
                {
                    _httpMessageHandler = value;
                    LastHttpClient = null;
                }
            }
        }

        public const string METRICS_CONTEXT_NAME = "ServiceProxy";

        private ICertificateLocator CertificateLocator { get; }

        private ILog Log { get; }
        private ServiceDiscoveryConfig GetConfig() => GetDiscoveryConfig().Services[ServiceName];
        private Func<DiscoveryConfig> GetDiscoveryConfig { get; }
        private JsonExceptionSerializer ExceptionSerializer { get; }

        private IEventPublisher<ClientCallEvent> EventPublisher { get; }

        private object HttpClientLock { get; } = new object();
        private HttpClient LastHttpClient { get; set; }
        private Tuple<bool, string, TimeSpan?> LastHttpClientKey { get; set; }

        private bool Disposed { get; set; }

        public ServiceProxyProvider(string serviceName, IEventPublisher<ClientCallEvent> eventPublisher,
            ICertificateLocator certificateLocator,
            ILog log,
            Func<string, ReachabilityChecker, IServiceDiscovery> serviceDiscoveryFactory,
            Func<DiscoveryConfig> getConfig,
            JsonExceptionSerializer exceptionSerializer)
        {
            EventPublisher = eventPublisher;
            CertificateLocator = certificateLocator;

            Log = log;

            ServiceName = serviceName;
            GetDiscoveryConfig = getConfig;
            ExceptionSerializer = exceptionSerializer;

            ServiceDiscovery = serviceDiscoveryFactory(serviceName, IsReachable);
        }




        /// <summary>
        /// Sets the length of time to wait for a HTTP request before aborting the request.
        /// </summary>
        /// <param name="timeout">The maximum length of time to wait.</param>
        public void SetHttpTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        private HttpClient GetHttpClient(ServiceDiscoveryConfig config)
        {
            lock (HttpClientLock)
            {
                bool useHttps = config.UseHttpsOverride ?? UseHttpsDefault;
                string securityRole = config.SecurityRole;
                var httpKey = Tuple.Create(useHttps, securityRole, config.RequestTimeout);

                if (LastHttpClient != null && LastHttpClientKey.Equals(httpKey))
                    return LastHttpClient;

                if (useHttps)
                    InitHttps(securityRole);

                LastHttpClientKey = httpKey;
                LastHttpClient = new HttpClient(HttpMessageHandler);
                TimeSpan? timeout = Timeout ?? config.RequestTimeout;
                if (timeout.HasValue)
                    LastHttpClient.Timeout = timeout.Value;
                return LastHttpClient;
            }
        }


        private void InitHttps(string securityRole)
        {
            if (HttpMessageHandler == null)
                HttpMessageHandler = new SocketsHttpHandler();

            var wrh = HttpMessageHandler as SocketsHttpHandler;

            if (wrh == null)
                throw new ProgrammaticException("When using HTTPS in ServiceProxy, only WebRequestHandler is supported.", unencrypted: new Tags { { "HandlerType", HttpMessageHandler.GetType().FullName } });

            var clientCert = CertificateLocator.GetCertificate("Client");
            var clientRootCertHash = clientCert.GetHashOfRootCertificate();

            wrh.SslOptions.ClientCertificates.Add(clientCert);

            wrh.SslOptions.RemoteCertificateValidationCallback = (sender, serverCertificate, serverChain, errors) =>
            {
                switch (errors)
                {
                    case SslPolicyErrors.RemoteCertificateNotAvailable:
                        Log.Error("Remote certificate not available.");
                        return false;
                    case SslPolicyErrors.RemoteCertificateChainErrors:
                        Log.Error(log =>
                        {
                            var sb = new StringBuilder("Certificate error/s.");
                            foreach (var chainStatus in serverChain.ChainStatus)
                            {
                                sb.AppendFormat("Status {0}, status information {1}\n", chainStatus.Status, chainStatus.StatusInformation);
                            }
                            log(sb.ToString());
                        });
                        return false;
                    case SslPolicyErrors.RemoteCertificateNameMismatch: // by design domain name do not match name of certificate, so RemoteCertificateNameMismatch is not an error.
                    case SslPolicyErrors.None:
                        //Check if security role of a server is as expected
                        if (securityRole != null)
                        {
                            var name = ((X509Certificate2)serverCertificate).GetNameInfo(X509NameType.SimpleName, false);

                            if (name == null || !name.Contains(securityRole))
                            {
                                return false;
                            }
                        }

                        bool hasSameRootCertificateHash = serverChain.HasSameRootCertificateHash(clientRootCertHash);

                        if (!hasSameRootCertificateHash)
                            Log.Error(_ => _("Server root certificate do not match client root certificate"));

                        return hasSameRootCertificateHash;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(errors), errors, "The supplied value of SslPolicyErrors is invalid.");
                }
            };
        }


        private async Task<bool> IsReachable(IEndPointHandle endpoint)
        {
            try
            {
                var config = GetConfig();
                var port = GetEffectivePort(endpoint, config);
                if (port == null)
                    return false;
                var uri = BuildUri(endpoint.HostName, port.Value, config);
                var response = await GetHttpClient(config).GetAsync(uri, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                return response.Headers.Contains(HSHttpHeaders.ServerHostname);
            }
            catch
            {
                return false;
            }
        }


        private string BuildUri(string hostName, int port, ServiceDiscoveryConfig config)
        {
            var useHttps = config.UseHttpsOverride ?? UseHttpsDefault;
            var urlTemplate = useHttps ? "https://{0}:{1}/" : "http://{0}:{1}/";
            return string.Format(urlTemplate, hostName, port);
        }

        private int? GetEffectivePort(IEndPointHandle endpoint, ServiceDiscoveryConfig config)
        {
            return endpoint.Port ?? DefaultPort ?? config.DefaultPort;
        }


        public virtual Task<object> Invoke(HttpServiceRequest request, Type resultReturnType)
        {
            return Invoke(request, resultReturnType, JsonSettings);
        }

        public virtual async Task<object> Invoke(HttpServiceRequest request, Type resultReturnType, JsonSerializerSettings jsonSettings)
        {
            return await InvokeCore(request, resultReturnType, jsonSettings).ConfigureAwait(false);
        }

        private async Task<object> InvokeCore(HttpServiceRequest request, Type resultReturnType, JsonSerializerSettings jsonSettings)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            request.Overrides = TracingContext.TryGetOverrides();
            request.TracingData = new TracingData
            {
                HostName = CurrentApplicationInfo.HostName?.ToUpperInvariant(),
                ServiceName = CurrentApplicationInfo.Name,
                RequestID = TracingContext.TryGetRequestID(),
                SpanID = Guid.NewGuid().ToString("N"), //Each call is new span                
                ParentSpanID = TracingContext.TryGetSpanID(),
                SpanStartTime = DateTimeOffset.UtcNow,
                AbandonRequestBy = TracingContext.AbandonRequestBy
            };
            PrepareRequest?.Invoke(request);
            var requestContent = JsonConvert.SerializeObject(request, jsonSettings);

            while (true)
            {
                var config = GetConfig();
                var clientCallEvent = EventPublisher.CreateEvent();
                clientCallEvent.TargetService = ServiceName;
                clientCallEvent.RequestId = request.TracingData?.RequestID;
                clientCallEvent.TargetMethod = request.Target.MethodName;
                clientCallEvent.SpanId = request.TracingData?.SpanID;
                clientCallEvent.ParentSpanId = request.TracingData?.ParentSpanID;

                string responseContent;
                HttpResponseMessage response;
                IEndPointHandle endPoint = await ServiceDiscovery.GetNextHost(clientCallEvent.RequestId).ConfigureAwait(false);

                int? effectivePort = GetEffectivePort(endPoint, config);
                if (effectivePort == null)
                    throw new ConfigurationException("Cannot access service. Service Port not configured. See tags to find missing configuration", unencrypted: new Tags {
                        {"ServiceName", ServiceName },
                        {"Required configuration key", $"Discovery.{ServiceName}.DefaultPort"}
                    });

                // The URL is only for a nice experience in Fiddler, it's never parsed/used for anything.
                var uri = BuildUri(endPoint.HostName, effectivePort.Value, config) + ServiceName;
                if (request.Target.MethodName != null)
                    uri += $".{request.Target.MethodName}";
                if (request.Target.Endpoint != null)
                    uri += $"/{request.Target.Endpoint}";

                try
                {
                    Log.Debug(_ => _("ServiceProxy: Calling remote service. See tags for details.",
                                  unencryptedTags: new
                                  {
                                      remoteEndpoint = endPoint.HostName,
                                      remotePort = effectivePort,
                                      remoteServiceName = ServiceName,
                                      remoteMethodName = request.Target.MethodName
                                  }));

                    clientCallEvent.TargetHostName = endPoint.HostName;
                    clientCallEvent.TargetPort = effectivePort.Value;

                    var httpContent = new StringContent(requestContent, Encoding.UTF8, "application/json");
                    httpContent.Headers.Add(HSHttpHeaders.ProtocolVersion, HttpServiceRequest.ProtocolVersion);

                    clientCallEvent.RequestStartTimestamp = Stopwatch.GetTimestamp();
                    try
                    {
                        response = await GetHttpClient(config).PostAsync(uri, httpContent).ConfigureAwait(false);
                        responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    finally
                    {
                        clientCallEvent.ResponseEndTimestamp = Stopwatch.GetTimestamp();
                    }
                    if (response.Headers.TryGetValues(HSHttpHeaders.ExecutionTime, out IEnumerable<string> values))
                    {
                        var time = values.FirstOrDefault();
                        if (TimeSpan.TryParse(time, out TimeSpan executionTime))
                        {
                            clientCallEvent.ServerTimeMs = executionTime.TotalMilliseconds;
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Log.Error("The remote service failed to return a valid HTTP response. Continuing to next " +
                              "host. See tags for URL and exception for details.",
                        exception: ex,
                        unencryptedTags: new { uri });

                    endPoint.ReportFailure(ex);
                    clientCallEvent.Exception = ex;
                    EventPublisher.TryPublish(clientCallEvent); // fire and forget!
                    continue;
                }
                catch (TaskCanceledException ex)
                {
                    Exception rex = new RemoteServiceException("The request to the remote service exceeded the " +
                                                               "allotted timeout. See the 'RequestUri' property on this exception for the URL that was " +
                                                               "called and the tag 'requestTimeout' for the configured timeout.",
                        uri,
                        ex,
                        unencrypted: new Tags
                        {
                            {"requestTimeout", LastHttpClient?.Timeout.ToString()},
                            {"requestUri", uri}
                        });

                    clientCallEvent.Exception = rex;

                    EventPublisher.TryPublish(clientCallEvent); // fire and forget!
                    throw rex;
                }

                if (response.Headers.Contains(HSHttpHeaders.ServerHostname) || response.Headers.Contains(HSHttpHeaders.ProtocolVersion))
                {
                    try
                    {
                        endPoint.ReportSuccess();

                        if (response.IsSuccessStatusCode)
                        {
                            var returnObj = JsonConvert.DeserializeObject(responseContent, resultReturnType, jsonSettings);

                            clientCallEvent.ErrCode = 0;
                            EventPublisher.TryPublish(clientCallEvent); // fire and forget!

                            return returnObj;
                        }
                        else
                        {
                            Exception remoteException;

                            try
                            {
                                remoteException = ExceptionSerializer.Deserialize(responseContent);
                            }
                            catch (Exception ex)
                            {
                                throw new RemoteServiceException("The remote service returned a failure response " +
                                                                 "that failed to deserialize.  See the 'RequestUri' property on this exception " +
                                                                 "for the URL that was called, the inner exception for the exact error and the " +
                                                                 "'responseContent' encrypted tag for the original response content.",
                                    uri,
                                    ex,
                                    unencrypted: new Tags { { "requestUri", uri } },
                                    encrypted: new Tags { { "responseContent", responseContent } });
                            }

                            clientCallEvent.Exception = remoteException;
                            EventPublisher.TryPublish(clientCallEvent); // fire and forget!

                            if (remoteException is RequestException || remoteException is EnvironmentException)
                                ExceptionDispatchInfo.Capture(remoteException).Throw();

                            throw new RemoteServiceException("The remote service returned a failure response. See " +
                                                             "the 'RequestUri' property on this exception for the URL that was called, and the " +
                                                             "inner exception for details.",
                                uri,
                                remoteException,
                                unencrypted: new Tags { { "requestUri", uri } });
                        }
                    }
                    catch (JsonException ex)
                    {
                        Log.Error("The remote service returned a response with JSON that failed " +
                                         "deserialization. See the 'uri' tag for the URL that was called, the exception for the " +
                                         "exact error and the 'responseContent' encrypted tag for the original response content.",
                                      exception: ex,
                                      unencryptedTags: new { uri },
                                      encryptedTags: new { responseContent });

                        clientCallEvent.Exception = ex;
                        EventPublisher.TryPublish(clientCallEvent); // fire and forget!
                        throw new RemoteServiceException("The remote service returned a response with JSON that " +
                                                         "failed deserialization. See the 'RequestUri' property on this exception for the URL " +
                                                         "that was called, the inner exception for the exact error and the 'responseContent' " +
                                                         "encrypted tag for the original response content.",
                            uri,
                            ex,
                            new Tags { { "responseContent", responseContent } },
                            new Tags { { "requestUri", uri } });
                    }
                }
                else
                {
                    var exception = response.StatusCode == HttpStatusCode.ServiceUnavailable ?
                        new Exception($"The remote service is unavailable (503) and is not recognized as a HS host at uri: {uri}") :
                        new Exception($"The remote service returned a response but is not recognized as a HS host at uri: {uri}");

                    endPoint.ReportFailure(exception);

                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        Log.Error("The remote service is unavailable (503) and is not recognized as a HS host. Continuing to next host.", unencryptedTags: new { uri });
                    else
                        Log.Error("The remote service returned a response but is not recognized as a HS host. Continuing to next host.", unencryptedTags: new { uri, statusCode = response.StatusCode }, encryptedTags: new { responseContent });

                    clientCallEvent.ErrCode = 500001;//(int)GSErrors.General_Server_Error;
                    EventPublisher.TryPublish(clientCallEvent); // fire and forget!
                }
            }
        }

        public async Task<ServiceSchema> GetSchema()
        {
            var result = await InvokeCore(new HttpServiceRequest { Target = new InvocationTarget { Endpoint = "schema" } }, typeof(ServiceSchema), JsonSettings).ConfigureAwait(false);
            return (ServiceSchema)result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                LastHttpClient?.Dispose();
                _httpMessageHandler.Dispose();
            }

            Disposed = true;
        }
    }
}
