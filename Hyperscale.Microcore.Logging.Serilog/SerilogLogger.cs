using Hyperscale.Microcore.SharedLogic.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hyperscale.Microcore.SharedLogic.Events;

namespace Hyperscale.Microcore.Logging.Serilog
{
    public class SerilogLogger: LogBase
    {
        public override TraceEventType? MinimumTraceLevel { get; set; }

        public SerilogLogger(Type receivingType)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();

            AssemblyName reflectedAssembly = receivingType.Assembly.GetName();
            CallSiteInfoTemplate = new LogCallSiteInfo
            {
                ReflectedType = receivingType,
                ClassName = receivingType.Name,
                AssemblyName = reflectedAssembly.Name,
                AssemblyVersion = reflectedAssembly.Version.ToString(),
            };
        }

        protected override Task<bool> WriteLog(TraceEventType level, LogCallSiteInfo logCallSiteInfo,
            string message, IDictionary<string, string> encryptedTags, IDictionary<string, string> unencryptedTags,
            Exception exception = null, string stackTrace = null)
        {
            var logLevel = ToLogLevel(level);
            if (Log.Logger.IsEnabled(logLevel))
            {
                var messageWithTags = message + ". " + string.Join(", ", unencryptedTags.Select(kvp => $"{kvp.Key.Substring(5)}={EventFieldFormatter.SerializeFieldValue(kvp.Value)}")) + ". ";
                Log.Logger.Write(logLevel, exception, messageWithTags);
            }

            return Task.FromResult(true);
        }

        private LogEventLevel ToLogLevel(TraceEventType traceEventType)
        {
            switch (traceEventType)
            {
                case TraceEventType.Critical: return LogEventLevel.Fatal;
                case TraceEventType.Error: return LogEventLevel.Error;
                case TraceEventType.Warning: return LogEventLevel.Warning;
                case TraceEventType.Information: return LogEventLevel.Information;
                case TraceEventType.Verbose: return LogEventLevel.Debug;
                default: return LogEventLevel.Verbose;
            }
        }
    }
}
