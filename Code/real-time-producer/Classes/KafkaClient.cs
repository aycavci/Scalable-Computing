using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Avro.Generic;
using Microsoft.Extensions.Options;


namespace real_time_producer
{
    public class KafkaClient
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly ILogger<KafkaClient> _logger;
        private ProducerConfig _producerConfig;
        private CachedSchemaRegistryClient _schemaRegistry;


        public KafkaClient(IOptions<KafkaOptions> kafkaOptions,
            ILogger<KafkaClient> logger)
        {
            _kafkaOptions = kafkaOptions.Value;
            _logger = logger;

            _producerConfig = new ProducerConfig() {
                BootstrapServers = kafkaOptions.Value.BootstrapServers
            };

            _schemaRegistry = new CachedSchemaRegistryClient(
                new SchemaRegistryConfig()
                {
                    Url = kafkaOptions.Value.SchemaRegistry
                }
            );

        }



        public async Task Produce(List<ActorItem> records, CancellationToken cancellationToken)
        {
            using (var producer = new ProducerBuilder<string, ActorItem>(_producerConfig)
                    .SetValueSerializer(new AvroSerializer<ActorItem>(_schemaRegistry))
                    .Build()
                )
            {
                foreach (ActorItem record in records)
                {
                    try
                    {
                        var message = new Message<string, ActorItem>()
                        {
                            Key = record.nconst,
                            Value = record,
                            Headers = new Headers()
                        };

                        message.Headers.Add("application-source", Encoding.UTF8.GetBytes(_kafkaOptions.MessageHeaderApplicationSource));

                        await producer.ProduceAsync(_kafkaOptions.ActorItemTopic
                            , message, cancellationToken)
                            .ContinueWith((task) =>
                            {
                                if (task.IsFaulted)
                                {
                                    _logger.LogError(task.Exception, "Error producing AVRO record to kafka: " + record.nconst);
                                }
                                else
                                {
                                    _logger.LogInformation("Produced {0} with id {1}", typeof(ActorItem).Name, record.nconst);
                                }
                            });
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Something happend while producing a message");
                    }

                }
                producer.Flush();
            }
        }


    }
}
