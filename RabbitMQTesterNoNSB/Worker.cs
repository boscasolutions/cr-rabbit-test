using CRB.RabbitMQTest.Contracts;
using CRB.RabbitMQTest.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQTesterNoNSB
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQOptions _options;
        private IConnection _connection;
        private IModel _channel;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _pumpTask;

        //private AsyncEventingBasicConsumer _consumer;

        public Worker(ILogger<Worker> logger, IOptions<RabbitMQOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var factory = new ConnectionFactory();
            factory.UserName = _options.UserName;
            factory.Password = _options.Password;
            factory.VirtualHost = _options.VirtualHost;
            factory.HostName = _options.HostName;
            factory.Port = factory.HostName == "localhost" ? 5672 : 5671;

            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_options.EndpointName, ExchangeType.Fanout, true);
            _channel.QueueDeclare(_options.EndpointName, true, false, false, null);
            _channel.QueueBind(_options.EndpointName, _options.EndpointName, "none");

            //_consumer = new AsyncEventingBasicConsumer(_channel);

            //_consumer.Received += Consumer_Received;

            //var tag = _channel.BasicConsume(_options.EndpointName, false, _consumer);

            _pumpTask = Task.Factory.StartNew(MessagePump, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

            return Task.CompletedTask;
        }

        private async Task MessagePump(object obj)
        {
            var cancellationToken = (CancellationToken) obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = _channel.BasicGet(_options.EndpointName, false);
                if (result != null)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var bodyString = Encoding.UTF8.GetString(result.Body.Span);
                            var start = bodyString.IndexOf('{');
                            var end = bodyString.IndexOf('}')+1;
                            bodyString = bodyString[start..end];
                            var testMessage = JsonConvert.DeserializeObject<TestMessage>(bodyString);

                            await Handle(testMessage);

                            _channel.BasicAck(result.DeliveryTag, false);
                        }
                        catch (Exception)
                        {
                            _channel.BasicNack(result.DeliveryTag, false, true);
                        }
                    });
                }
            };
        }

        //private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        //{
        //    var bodyString = Encoding.UTF8.GetString(e.Body.Span);
        //    var testMessage = JsonConvert.DeserializeObject<TestMessage>(bodyString);

        //    await Handle(testMessage);

        //    _channel.BasicAck(e.DeliveryTag, false);
        //}

        public Task Handle(TestMessage message)
        {
            if (message.MillisecondsOfWork > 0)
            {
                try
                {
                    var cancellationTokenSource = new CancellationTokenSource(message.MillisecondsOfWork);
                    WorkHelper.FindPrimeNumbers(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException) { }
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            await _pumpTask;

            _channel.Close();
            _connection.Close();
        }
    }
}
