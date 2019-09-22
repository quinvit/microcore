using System;
using AutoMapper;
using Hyperscale.Microcore.Logging.Serilog;
using Hyperscale.Microcore.Ninject;
using Hyperscale.Microcore.Ninject.Host;
using Hyperscale.Microcore.SharedLogic;
using Microsoft.Extensions.Configuration;
using Ninject;
using ReportService.Entities;
using ReportService.Interface;
using ReportService.Interface.Models;

namespace ReportService
{

    class ReportServiceHost : MicrocoreServiceHost<IReportService>
    {
        private IConfiguration _configuration;

        public ReportServiceHost(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override ILoggingModule GetLoggingModule() => new SerilogModule();

        protected override void Configure(IKernel kernel, BaseCommonConfig commonConfig)
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<TimeRecord, TimeRecordEntity>();
                cfg.CreateMap<TimeRecordEntity, TimeRecord>();
            });

            kernel.Bind<IMapper>().ToMethod(x => config.CreateMapper()).InSingletonScope();

            kernel.Bind<IConfiguration>().ToConstant(_configuration);
            kernel.Bind<IReportService>().To<ReportService>();
        }
    }
}
