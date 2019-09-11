using System;
using CalculatorService.Interface;
using Hyperscale.Microcore.Logging.Serilog;
using Hyperscale.Microcore.Ninject;
using Hyperscale.Microcore.SharedLogic;
using Ninject;
using Serilog;

namespace CalculatorService.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("HS_CONFIG_ROOT", Environment.CurrentDirectory);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();

            try
            {
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
