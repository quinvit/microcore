using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TimeTrackerAPI
{
    public class Program
    {
        static List<string> urls = new List<string>();
        public static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("HS_CONFIG_ROOT", Environment.CurrentDirectory);

            urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(';').ToList();
            if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out int port))
            {
                urls.Add($"http://0.0.0.0:{port}");
            }

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(urls.ToArray())
                .UseStartup<Startup>();
    }
}
