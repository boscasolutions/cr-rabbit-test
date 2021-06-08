namespace RabbitMQTesterNoNSB
{
    public class RabbitMQOptions
    {
        public string HostName { get; internal set; }
        public string UserName { get; internal set; }
        public string Password { get; internal set; }
        public string VirtualHost { get; internal set; }
        public string EndpointName { get; internal set; }
        public string ErrorQueue { get; internal set; }
        public string AuditQueue { get; internal set; }
    }
}