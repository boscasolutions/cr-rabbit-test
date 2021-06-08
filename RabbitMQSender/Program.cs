using CRB.RabbitMQTest.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQSender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .UseNServiceBus(context =>
                {
                    var c = context.Configuration;
                    var endpointName = c.GetValue<string>("EndpointName");

                    var rabbitConnectionString = c.GetConnectionString("RabbitMQ");

                    var configuration = new EndpointConfiguration("SendOnly");

                    configuration.LicensePath("NsbLicense2019.xml");
                    configuration.EnableInstallers();
                    configuration.UseSerialization<NewtonsoftSerializer>();
                    configuration.SendOnly();

                    var transport = configuration.UseTransport<RabbitMQTransport>();
                    transport.UseConventionalRoutingTopology();
                    transport.ConnectionString(rabbitConnectionString);

                    var routing = transport.Routing();
                    routing.RouteToEndpoint(typeof(TestMessage), endpointName);

                    var convensions = configuration.Conventions();
                    convensions.DefiningMessagesAs(type => type.Namespace.EndsWith(".Messages"));

                    return configuration;
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<SenderOptions>(o =>
                    {
                        if (!int.TryParse(args.FirstOrDefault(), out var n))
                            n = 100;

                        o.NumberToSend = n;
                    });

                    services.AddHostedService<Worker>();
                });
    }
}
