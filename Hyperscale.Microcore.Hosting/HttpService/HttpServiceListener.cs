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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hyperscale.Common.Contracts;
using Hyperscale.Common.Contracts.Exceptions;
using Hyperscale.Common.Contracts.HttpService;
using Hyperscale.Microcore.Hosting.Events;
using Hyperscale.Microcore.Hosting.HttpService.Endpoints;
using Hyperscale.Microcore.Interfaces.Events;
using Hyperscale.Microcore.Interfaces.Logging;
using Hyperscale.Microcore.Interfaces.SystemWrappers;
using Hyperscale.Microcore.SharedLogic;
using Hyperscale.Microcore.SharedLogic.Configurations;
using Hyperscale.Microcore.SharedLogic.Events;
using Hyperscale.Microcore.SharedLogic.Exceptions;
using Hyperscale.Microcore.SharedLogic.HttpService;
using Hyperscale.Microcore.SharedLogic.Measurement;
using Hyperscale.Microcore.SharedLogic.Security;
using Newtonsoft.Json;


// ReSharper disable ConsiderUsingConfigureAwait

namespace Hyperscale.Microcore.Hosting.HttpService
{
    public sealed class HttpServiceListener : IDisposable
    {
        private readonly IServerRequestPublisher _serverRequestPublisher;

        private static JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DateParseHandling = DateParseHandling.None
        };

        private static JsonSerializerSettings JsonSettingsWeak { get; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DateParseHandling = DateParseHandling.None
        };

        private string Prefix { get; }
        private byte[] ServerRootCertHash { get; }

        private IActivator Activator { get; }
        private IWorker Worker { get; }
        private IServiceEndPointDefinition ServiceEndPointDefinition { get; }
        private HttpListener Listener { get; }
        private ILog Log { get; }
        private IEventPublisher<ServiceCallEvent> EventPublisher { get; }
        private IEnumerable<ICustomEndpoint> CustomEndpoints { get; }
        private JsonExceptionSerializer ExceptionSerializer { get; }
        private Func<LoadShedding> LoadSheddingConfig { get; }

        private ServiceSchema ServiceSchema { get; }

        public HttpServiceListener(IActivator activator, IWorker worker, IServiceEndPointDefinition serviceEndPointDefinition,
                                   ICertificateLocator certificateLocator, ILog log, IEventPublisher<ServiceCallEvent> eventPublisher,
                                   IEnumerable<ICustomEndpoint> customEndpoints,
                                   JsonExceptionSerializer exceptionSerializer, 
                                   ServiceSchema serviceSchema,                                   
                                   Func<LoadShedding> loadSheddingConfig,
                                   IServerRequestPublisher serverRequestPublisher)

        {
            ServiceSchema = serviceSchema;
            _serverRequestPublisher = serverRequestPublisher;
            ServiceEndPointDefinition = serviceEndPointDefinition;
            Worker = worker;
            Activator = activator;
            Log = log;
            EventPublisher = eventPublisher;
            CustomEndpoints = customEndpoints.ToArray();
            ExceptionSerializer = exceptionSerializer;
            LoadSheddingConfig = loadSheddingConfig;

            if (serviceEndPointDefinition.UseSecureChannel)
                ServerRootCertHash = certificateLocator.GetCertificate("Service").GetHashOfRootCertificate();

            var urlPrefixTemplate = ServiceEndPointDefinition.UseSecureChannel ? "https://+:{0}/" : "http://+:{0}/";
            Prefix = string.Format(urlPrefixTemplate, ServiceEndPointDefinition.HttpPort);

            Listener = new HttpListener
            {
                IgnoreWriteExceptions = true,
                Prefixes = { Prefix }
            };

        }


        public void Start()
        {
            try
            {
                Listener.Start();
                Log.Info(_ => _("HttpServiceListener started", unencryptedTags: new { prefix = Prefix }));
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode != 5)
                    throw;

                throw new Exception(
                    "One or more of the specified HTTP listen ports wasn't configured to run without administrative permissions.\n" +
                    "To configure them, run the following commands in an elevated (administrator) command prompt:\n" +
                    $"netsh http add urlacl url={Prefix} user={CurrentApplicationInfo.OsUser}");
            }

            StartListening();
        }


        private async void StartListening()
        {
            while (Listener.IsListening)
            {
                HttpListenerContext context;

                try
                {
                    context = await Listener.GetContextAsync();
                    Worker.FireAndForget(() => HandleRequest(context));
                }
                catch (ObjectDisposedException)
                {
                    break; // Listener has been stopped, GetContextAsync() is aborted.
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 995)
                        break;

                    Log.Error(_ => _("An error has occured during HttpListener.GetContextAsync(). Stopped listening to additional requests.", exception: ex));
                }
                catch (Exception ex)
                {
                    Log.Error(_ => _("An error has occured during HttpListener.GetContextAsync(). Stopped listening to additional requests.", exception: ex));
                    throw;
                }
            }
        }


        private async Task HandleRequest(HttpListenerContext context)
        {
            RequestTimings.ClearCurrentTimings();
            using (context.Response)
            {
                var sw = Stopwatch.StartNew();
                Exception ex;

                // Special endpoints should not be logged/measured/traced like regular endpoints
                try
                {
                    foreach (var customEndpoint in CustomEndpoints)
                    {
                        if (await customEndpoint.TryHandle(context, (data, status, type) => TryWriteResponse(context, data, status, type)))
                        {
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    ex = GetRelevantException(e);
                    await TryWriteResponse(context, ExceptionSerializer.Serialize(ex), GetExceptionStatusCode(ex));
                    return;
                }

                // Regular endpoint handling
                TracingContext.SetUpStorage();
                RequestTimings.GetOrCreate(); // initialize request timing context

                Exception actualException = null;
                string methodName = null;
                // Initialize with empty object for protocol backwards-compatibility.

                var requestData = new HttpServiceRequest { TracingData = new TracingData() };
                ServiceMethod serviceMethod = null;
                try
                {
                    try
                    {
                        ValidateRequest(context);
                        await CheckSecureConnection(context);

                        requestData = await ParseRequest(context);

                        TracingContext.SetOverrides(requestData.Overrides);

                        serviceMethod = ServiceEndPointDefinition.Resolve(requestData.Target);
                        methodName = serviceMethod.ServiceInterfaceMethod.Name;
                    }
                    catch (Exception e)
                    {
                        actualException = e;
                        if (e is RequestException)
                            throw;

                        throw new RequestException("Invalid request", e);
                    }

                    RejectRequestIfLateOrOverloaded();

                    var responseJson = await GetResponse(context, serviceMethod, requestData);
                    await TryWriteResponse(context, responseJson);

                }
                catch (Exception e)
                {
                    actualException = actualException ?? e;
                    ex = GetRelevantException(e);

                    string json = ExceptionSerializer.Serialize(ex);
                    await TryWriteResponse(context, json, GetExceptionStatusCode(ex));
                }
                finally
                {
                    sw.Stop();
                    _serverRequestPublisher.TryPublish(requestData, actualException, serviceMethod, sw.Elapsed.TotalMilliseconds);
                }
            }
        }


        private void RejectRequestIfLateOrOverloaded()
        {
            var config = LoadSheddingConfig();
            var now = DateTimeOffset.UtcNow;

            // Too much time passed since our direct caller made the request to us; something's causing a delay. Log or reject the request, if needed.
            if (   config.DropMicrocoreRequestsBySpanTime != LoadShedding.Toggle.Disabled
                && TracingContext.SpanStartTime != null
                && TracingContext.SpanStartTime.Value + config.DropMicrocoreRequestsOlderThanSpanTimeBy < now)
            {

                if (config.DropMicrocoreRequestsBySpanTime == LoadShedding.Toggle.LogOnly)
                    Log.Warn(_ => _("Accepted Microcore request despite that too much time passed since the client sent it to us.", unencryptedTags: new {
                        clientSendTime    = TracingContext.SpanStartTime,
                        currentTime       = now,
                        maxDelayInSecs    = config.DropMicrocoreRequestsOlderThanSpanTimeBy.TotalSeconds,
                        actualDelayInSecs = (now -TracingContext.SpanStartTime.Value).TotalSeconds,
                    }));

                else if (config.DropMicrocoreRequestsBySpanTime == LoadShedding.Toggle.Drop)
                    throw new EnvironmentException("Dropping Microcore request since too much time passed since the client sent it to us.", unencrypted: new Tags {
                        ["clientSendTime"]    = TracingContext.SpanStartTime.ToString(),
                        ["currentTime"]       = now.ToString(),
                        ["maxDelayInSecs"]    = config.DropMicrocoreRequestsOlderThanSpanTimeBy.TotalSeconds.ToString(),
                        ["actualDelayInSecs"] = (now - TracingContext.SpanStartTime.Value).TotalSeconds.ToString(),
                    });
            }

            // Too much time passed since the API gateway initially sent this request till it reached us (potentially
            // passing through other micro-services along the way). Log or reject the request, if needed.
            if (   config.DropRequestsByDeathTime != LoadShedding.Toggle.Disabled
                && TracingContext.AbandonRequestBy != null
                && now > TracingContext.AbandonRequestBy.Value - config.TimeToDropBeforeDeathTime)
            {
                if (config.DropRequestsByDeathTime == LoadShedding.Toggle.LogOnly)
                    Log.Warn(_ => _("Accepted Microcore request despite exceeding the API gateway timeout.", unencryptedTags: new {
                        requestDeathTime = TracingContext.AbandonRequestBy,
                        currentTime      = now,
                        overTimeInSecs   = (now - TracingContext.AbandonRequestBy.Value).TotalSeconds,
                    }));

                else if (config.DropRequestsByDeathTime == LoadShedding.Toggle.Drop)
                    throw new EnvironmentException("Dropping Microcore request since the API gateway timeout passed.", unencrypted: new Tags {
                        ["requestDeathTime"] = TracingContext.AbandonRequestBy.ToString(),
                        ["currentTime"]      = now.ToString(),
                        ["overTimeInSecs"]   = (now - TracingContext.AbandonRequestBy.Value).TotalSeconds.ToString(),
                    });
            }
        }


        private static Exception GetRelevantException(Exception e)
        {
            if (e is RequestException)
                return e;

            var ex = GetAllExceptions(e).FirstOrDefault(x => (x is TargetInvocationException || x is AggregateException) == false);

            return ex;
        }

        private static IEnumerable<Exception> GetAllExceptions(Exception ex)
        {
            while (ex != null)
            {
                yield return ex;
                ex = ex.InnerException;
            }
        }

        private void ValidateRequest(HttpListenerContext context)
        {
            var clientVersion = context.Request.Headers[HSHttpHeaders.ProtocolVersion];

            if (clientVersion != null && clientVersion != HttpServiceRequest.ProtocolVersion)
            {
                throw new RequestException($"Client protocol version {clientVersion} is not supported by the server protocol version {HttpServiceRequest.ProtocolVersion}.");
            }

            if (context.Request.HttpMethod != "POST")
            {
                context.Response.Headers.Add("Allow", "POST");
                throw new RequestException("Only POST calls are allowed.");
            }

            if (context.Request.ContentType == null || context.Request.ContentType.StartsWith("application/json") == false)
            {
                context.Response.Headers.Add("Accept", "application/json");
                throw new RequestException("Only requests with content type 'application/json' are supported.");
            }

            if (context.Request.ContentLength64 == 0)
            {
                throw new RequestException("Only requests with content are supported.");
            }
        }


        private async Task CheckSecureConnection(HttpListenerContext context)
        {
            if (context.Request.IsSecureConnection != ServiceEndPointDefinition.UseSecureChannel)
            {
                throw new SecureRequestException("Incompatible channel security - both client and server must be either secure or insecure.", unencrypted: new Tags { { "serviceIsSecure", ServiceEndPointDefinition.UseSecureChannel.ToString() }, { "requestIsSecure", context.Request.IsSecureConnection.ToString() }, { "requestedUrl", context.Request.Url.ToString() } });
            }

            if (!context.Request.IsSecureConnection)
                return;

            var clientCertificate = await context.Request.GetClientCertificateAsync();

            if (clientCertificate == null)
            {
                throw new SecureRequestException("Client certificate is not present.");
            }

            var isValid = clientCertificate.HasSameRootCertificateHash(ServerRootCertHash);

            if (!isValid) // Invalid certificate
            {
                throw new SecureRequestException("Client certificate is not valid.");
            }
        }

        private async Task TryWriteResponse(HttpListenerContext context, string data, HttpStatusCode httpStatus = HttpStatusCode.OK, string contentType = "application/json")
        {
            context.Response.Headers.Add(HSHttpHeaders.ProtocolVersion, HttpServiceRequest.ProtocolVersion);

            var body = Encoding.UTF8.GetBytes(data ?? "");

            context.Response.StatusCode = (int)httpStatus;
            context.Response.ContentLength64 = body.Length;
            context.Response.ContentType = contentType;
            context.Response.Headers.Add(HSHttpHeaders.ServiceVersion, CurrentApplicationInfo.Version.ToString());
            context.Response.Headers.Add(HSHttpHeaders.ServerHostname, CurrentApplicationInfo.HostName);
            context.Response.Headers.Add(HSHttpHeaders.SchemaHash, ServiceSchema.Hash);

            try
            {
                await context.Response.OutputStream.WriteAsync(body, 0, body.Length);
            }
            catch (HttpListenerException writeEx)
            {
                // For some reason, HttpListener.IgnoreWriteExceptions doesn't work here.
                Log.Warn(_ => _("HttpServiceListener: Failed to write the response of a service call. See exception and tags for details.",
                    exception: writeEx,
                    unencryptedTags: new
                    {
                        remoteEndpoint = context.Request.RemoteEndPoint,
                        rawUrl = context.Request.RawUrl,
                        status = httpStatus
                    },
                    encryptedTags: new { response = data }));
            }
        }


        private async Task<HttpServiceRequest> ParseRequest(HttpListenerContext context)
        {
            HttpServiceRequest request;
            using (var streamReader = new StreamReader(context.Request.InputStream))
            {
                var json = await streamReader.ReadToEndAsync();
                request = JsonConvert.DeserializeObject<HttpServiceRequest>(json, JsonSettings);
            }

            request.TracingData = request.TracingData ?? new TracingData();
            request.TracingData.RequestID = request.TracingData.RequestID ?? Guid.NewGuid().ToString("N");

            TracingContext.SetRequestID(request.TracingData.RequestID);
            TracingContext.SetSpan(request.TracingData.SpanID, request.TracingData.ParentSpanID);
            TracingContext.SpanStartTime    = request.TracingData.SpanStartTime;
            TracingContext.AbandonRequestBy = request.TracingData.AbandonRequestBy;

            return request;
        }


        private async Task<string> GetResponse(HttpListenerContext context, ServiceMethod serviceMethod, HttpServiceRequest requestData)
        {
            var taskType = serviceMethod.ServiceInterfaceMethod.ReturnType;
            var resultType = taskType.IsGenericType ? taskType.GetGenericArguments().First() : null;
            var arguments = requestData.Target.IsWeaklyTyped ? GetParametersByName(serviceMethod, requestData.Arguments) : requestData.Arguments.Values.Cast<object>().ToArray();
            var settings = requestData.Target.IsWeaklyTyped ? JsonSettingsWeak : JsonSettings;

            var invocationResult = await Activator.Invoke(serviceMethod, arguments);
            string response = JsonConvert.SerializeObject(invocationResult.Result, resultType, settings);
            context.Response.Headers.Add(HSHttpHeaders.ExecutionTime, invocationResult.ExecutionTime.ToString());

            return response;
        }

        private static object[] GetParametersByName(ServiceMethod serviceMethod, IDictionary args)
        {
            return serviceMethod.ServiceInterfaceMethod
                .GetParameters()
                .Select(p => JsonHelper.ConvertWeaklyTypedValue(args[p.Name], p.ParameterType))
                .ToArray();
        }

        internal static HttpStatusCode GetExceptionStatusCode(Exception exception)
        {
            if (exception is SecureRequestException)
                return HttpStatusCode.Forbidden;
            if (exception is MissingMethodException)
                return HttpStatusCode.NotFound;
            if (exception is RequestException || exception is JsonException)
                return HttpStatusCode.BadRequest;
            if (exception is EnvironmentException)
                return HttpStatusCode.ServiceUnavailable;

            return HttpStatusCode.InternalServerError;
        }


        public void Dispose()
        {
            Worker.Dispose();
            Listener.Close();
        }
    }
}
