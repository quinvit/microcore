using System;
using CalculatorService.Interface;
using Hyperscale.Microcore.Logging.Serilog;
using Hyperscale.Microcore.Ninject;
using Hyperscale.Microcore.Ninject.Host;
using Hyperscale.Microcore.SharedLogic;
using Ninject;
using Serilog;

namespace CalculatorService
{

    class CalculatorServiceHost : MicrocoreServiceHost<ICalculatorService>
    {
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("HS_CONFIG_ROOT", Environment.CurrentDirectory);
            CurrentApplicationInfo.Init("CalculatorService");

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();

            try
            {
                new CalculatorServiceHost().Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        protected override ILoggingModule GetLoggingModule() => new SerilogModule();

        protected override void Configure(IKernel kernel, BaseCommonConfig commonConfig)
        {
            kernel.Bind<ICalculatorService>().To<CalculatorService>();
        }
    }
}
