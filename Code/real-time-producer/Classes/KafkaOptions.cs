namespace real_time_producer
{
    public class KafkaOptions
    {
        public const string SECTION_NAME = "Kafka";

        public string BootstrapServers {get;set;}
        public string SchemaRegistry {get;set;}

        public string MessageHeaderApplicationSource {get;set;}

        public string ActorItemTopic {get;set;}
    }
}
