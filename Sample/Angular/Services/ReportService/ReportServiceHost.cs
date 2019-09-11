using System;
using Hyperscale.Microcore.Logging.Serilog;
using Hyperscale.Microcore.Ninject;
using Hyperscale.Microcore.Ninject.Host;
using Hyperscale.Microcore.SharedLogic;
using Ninject;
using ReportService.Interface;

namespace ReportService
{

    class ReportServiceHost : MicrocoreServiceHost<IReportService>
    {
        protected override ILoggingModule GetLoggingModule() => new SerilogModule();

        protected override void Configure(IKernel kernel, BaseCommonConfig commonConfig)
        {
            kernel.Bind<IReportService>().To<ReportService>();
        }
    }
}
