using System;
using System.Threading.Tasks;
using HS.Microcore.Interfaces;
using HS.Microcore.Interfaces.Logging;
using App.Metrics;

namespace HS.Microcore.Fakes
{
    public sealed class MetricsInitializerFake : IMetricsInitializer
    {

        public void Init()
        {

        }

        public void Dispose()
        {
            try
            {
            }
            catch (AggregateException ae)
            {
                // Ignore all TaskCanceledExceptions (unhandled by Metrics.NET for unknown reasons)
                ae.Handle(ex => ex is TaskCanceledException);
            }
        }


        private IMetricsSettings MetricsSettings { get; set; }

        private ILog Log { get; }

        public MetricsInitializerFake(ILog log, IMetricsSettings metricsSettings)
        {
            MetricsSettings = metricsSettings;
            Log = log;
        }

    }
}
