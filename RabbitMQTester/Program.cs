using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace RabbitMQTester
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"FATAL: {ex}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseNServiceBus(context =>
                {
                    var c = context.Configuration;
                    var endpointName = c.GetValue<string>("EndpointName");
                    var errorQueue = c.GetValue<string>("ErrorQueue");
                    var auditQueue = c.GetValue<string>("AuditQueue");
                    var useOutbox = c.GetValue<bool>("UseOutbox");
                    var serviceControlQueue = c.GetValue<string>("ServiceControlQueue");
                    var serviceControlMetricsAddress = c.GetValue<string>("ServiceControlMetricsAddress");

                    var rabbitConnectionString = c.GetConnectionString("RabbitMQ");
                    var sqlConnectionString = c.GetConnectionString("SQL");

                    var configuration = new EndpointConfiguration(endpointName);

                    // REVIEW: This uses the default concurrency limit which is number of cores, that is fine as the current task is CPU bound but if the task as IO bound increasing concurrency could be beneficial. But... as we are doing IO when sending audit messages the concurrently limit should be increased too.
                    configuration.LimitMessageProcessingConcurrencyTo(Environment.ProcessorCount * 4);

                    configuration.LicensePath("NsbLicense2019.xml");
                    configuration.EnableInstallers();
                    configuration.UseSerialization<NewtonsoftSerializer>();

                    var transport = configuration.UseTransport<RabbitMQTransport>();
                    transport.UseConventionalRoutingTopology();
                    transport.ConnectionString(rabbitConnectionString);
                    // transport.PrefetchMultiplier(5);
                    // transport.PrefetchCount(5000);

                    configuration.SendFailedMessagesTo(errorQueue);
                    configuration.AuditProcessedMessagesTo(auditQueue);
                    configuration.SendHeartbeatTo(serviceControlQueue, frequency: TimeSpan.FromSeconds(15), timeToLive: TimeSpan.FromSeconds(30));
                    var metrics = configuration.EnableMetrics();
                    metrics.SendMetricDataToServiceControl(
                        serviceControlMetricsAddress: serviceControlMetricsAddress,
                        interval: TimeSpan.FromSeconds(10));

                    var convensions = configuration.Conventions();
                    convensions.DefiningMessagesAs(type => type.Namespace.EndsWith(".Messages"));

                    if (useOutbox)
                    {
                        var persistence = configuration.UsePersistence<SqlPersistence>();
                        persistence.SqlDialect<SqlDialect.MsSqlServer>();
                        persistence.ConnectionBuilder(() => new SqlConnection(sqlConnectionString));

                        configuration.EnableOutbox();
                    }

                    return configuration;
                }).ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                }); 
    }
}
