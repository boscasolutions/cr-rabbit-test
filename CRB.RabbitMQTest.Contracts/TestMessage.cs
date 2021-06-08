using System;

namespace CRB.RabbitMQTest.Messages
{
    public class TestMessage
    {
        public int MillisecondsOfWork { get; set; }
        public int TTL { get; set; }
    }
}
