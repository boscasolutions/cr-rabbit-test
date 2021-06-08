using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRB.RabbitMQTest.Messages;

namespace RabbitMQSender
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SenderOptions _options;
        private readonly IServiceProvider _serviceProvider;

        private IMessageSession _messageSession;
        private IMessageSession MessageSession => _messageSession ??= _serviceProvider.GetService<IMessageSession>();

        public Worker(ILogger<Worker> logger, IOptions<SenderOptions> options, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = options.Value;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var message = new TestMessage { MillisecondsOfWork = 0 };

            await Task.WhenAll(Enumerable.Range(0, _options.NumberToSend).Select(x => MessageSession.Send(message)));

            _logger.LogInformation($"sent {_options.NumberToSend} messages");
        }
    }
}
