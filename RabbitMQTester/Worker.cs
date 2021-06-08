using CRB.RabbitMQTest.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQTester
{
    internal class Worker : BackgroundService
    {
        //private readonly IServiceProvider _serviceProvider;
        //private IMessageSession MessageSession => _serviceProvider.GetService<IMessageSession>();

        //public Worker(IServiceProvider serviceProvider)
        //{
        //    _serviceProvider = serviceProvider;
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //for(var i=0;i < 10; i++)
            //    await MessageSession.SendLocal<TestMessage>(message =>
            //    {
            //        message.MillisecondsOfWork = 0;
            //    });
        }
    }
}