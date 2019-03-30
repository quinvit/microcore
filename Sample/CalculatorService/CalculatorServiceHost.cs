using System;
using CalculatorService.Interface;
using HS.Microcore.Logging.Serilog;
using HS.Microcore.Ninject;
using HS.Microcore.Ninject.Host;
using HS.Microcore.SharedLogic;
using Ninject;

namespace CalculatorService
{

    class CalculatorServiceHost : MicrocoreServiceHost<ICalculatorService>
    {
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("HS_CONFIG_ROOT", Environment.CurrentDirectory);
            Environment.SetEnvironmentVariable("HS_ENVVARS_FILE", Environment.CurrentDirectory);
            Environment.SetEnvironmentVariable("REGION", "vn");
            Environment.SetEnvironmentVariable("ZONE", "hcm");
            Environment.SetEnvironmentVariable("ENV", "dev");


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
