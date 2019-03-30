using System;
using CalculatorService.Interface;
using HS.Microcore.Logging.Serilog;
using HS.Microcore.Ninject;
using HS.Microcore.SharedLogic;
using Ninject;

namespace CalculatorService.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Environment.SetEnvironmentVariable("HS_CONFIG_ROOT", Environment.CurrentDirectory);
                Environment.SetEnvironmentVariable("HS_ENVVARS_FILE", Environment.CurrentDirectory);
                Environment.SetEnvironmentVariable("REGION", "vn");
                Environment.SetEnvironmentVariable("ZONE", "hcm");
                Environment.SetEnvironmentVariable("ENV", "dev");

                CurrentApplicationInfo.Init("CalculatorService.Client");

                var kernel = new StandardKernel();
                kernel.Load<MicrocoreModule>();
                kernel.Load<SerilogModule>();

                ICalculatorService calculatorService = kernel.Get<ICalculatorService>();
                int sum = calculatorService.Add(2, 3).Result;
                Console.WriteLine($"Sum: {sum}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
