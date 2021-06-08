using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQTesterNoNSB
{
    public class Program
    {
        private const string HOST = "host";
        private const string USERNAME = "username";
        private const string PASSWORD = "password";
        private const string VIRTUALHOST = "virtualhost";
        private const string DEFAULT_USERNAME = "guest";
        private const string DEFAULT_PASSWORD = "guest";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    var c = hostContext.Configuration;

                    var rabbitConnectionString = c.GetConnectionString("RabbitMQ");
                    var endpointName = c.GetValue<string>("EndpointName");
                    var errorQueue = c.GetValue<string>("ErrorQueue");
                    var auditQueue = c.GetValue<string>("AuditQueue");

                    services.Configure<RabbitMQOptions>(o =>
                    {
                        var d = rabbitConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                                        .ToDictionary(k => k.Split('=').First().ToLower().Trim(),
                                                                      k => k.Split('=').Last().Trim());
                        o.HostName = d.ContainsKey(HOST) ? d[HOST] : null;
                        o.UserName = d.ContainsKey(USERNAME) ? d[USERNAME] : DEFAULT_USERNAME;
                        o.Password = d.ContainsKey(PASSWORD) ? d[PASSWORD] : DEFAULT_PASSWORD;
                        o.VirtualHost = d.ContainsKey(VIRTUALHOST) ? d[VIRTUALHOST] : "/";
                        o.EndpointName = endpointName;
                        o.ErrorQueue = errorQueue;
                        o.AuditQueue = auditQueue;
                    });


                    services.AddHostedService<Worker>();
                });
    }
}
