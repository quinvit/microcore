using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hyperscale.Microcore.SharedLogic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ReportService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("HS_CONFIG_ROOT", Environment.CurrentDirectory);
            CurrentApplicationInfo.Init("TimeTracker-ReportService");

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                Task.Factory.StartNew(() => new ReportServiceHost().Run(), TaskCreationOptions.AttachedToParent)
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine(task.Exception.ToString());
                        }
                    });
                CreateWebHostBuilder(args).Build().Run();
            }
            else
            {
                new ReportServiceHost().Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
