using Confluent.Kafka;
using System.Text.Json;

namespace SuperBodega.API.Infrastructure.Messaging;

public interface IKafkaProducerService
{
    Task PublishAsync<T>(string topic, T data, CancellationToken ct = default);
}

public sealed class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    public KafkaProducerService(IConfiguration cfg)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = cfg["KAFKA_BOOTSTRAP"]!,               
            ClientId = "superbodega-producer",
            EnableIdempotence = true                                    
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }
    public Task PublishAsync<T>(string topic, T data, CancellationToken ct = default)
        => _producer.ProduceAsync(topic,
            new Message<Null, string> { Value = JsonSerializer.Serialize(data) }, ct);
    public void Dispose() => _producer?.Dispose();
}
