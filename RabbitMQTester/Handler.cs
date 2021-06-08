using CRB.RabbitMQTest.Contracts;
using CRB.RabbitMQTest.Messages;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQTester
{
    public class Handler : IHandleMessages<TestMessage>
    {
        public async Task Handle(TestMessage message, IMessageHandlerContext context)
        {
            --message.TTL;

            if (message.TTL > 0)
            {
                await context.SendLocal(message);
            }

            if (message.MillisecondsOfWork > 0)
            {
                await Task.Run(() =>
                {
                    if (message.MillisecondsOfWork > 0)
                    {
                        try
                        {
                            var cancellationTokenSource = new CancellationTokenSource(message.MillisecondsOfWork);
                            WorkHelper.FindPrimeNumbers(cancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                        }
                    }
                });
            }
        }
    }
}
