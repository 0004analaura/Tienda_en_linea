using Confluent.Kafka;
using System.Text.Json;
using SuperBodega.API.Services.Ecommerce;
using SuperBodega.API.DTOs.Ecommerce;
using SuperBodega.API.Infrastructure.Email;
using SuperBodega.API.Data;

namespace SuperBodega.API.Infrastructure.Messaging;

public class KafkaConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<KafkaConsumerHostedService> _log;
    private readonly IConfiguration _cfg;

    public KafkaConsumerHostedService(IServiceProvider sp, ILogger<KafkaConsumerHostedService> log, IConfiguration cfg)
    {
        _sp = sp; 
        _log = log;
        _cfg = cfg; 
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        _ = Task.Run(() => Loop(ct), ct);     
        return Task.CompletedTask;             
    }

    private async Task Loop(CancellationToken ct)
    {
        var conf = new ConsumerConfig
        {
            BootstrapServers = _cfg["KAFKA_BOOTSTRAP"]!,
            GroupId = "superbodega-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest
    };

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(conf).Build();
                consumer.Subscribe("compras");
                _log.LogInformation("Kafka conectado a topic 'compras'");

                while (!ct.IsCancellationRequested)
                {
                    var cr = consumer.Consume(ct);
                    if (cr?.Message?.Value is null) continue;

                    var msg = JsonSerializer.Deserialize<CarritoCheckoutMessage>(cr.Message.Value);
                    if (msg is null) continue;


                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<SuperBodegaContext>();
                    var carritoSvc = scope.ServiceProvider.GetRequiredService<CarritoService>();
                    var emailSvc = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var ventaId = await carritoSvc.RealizarCompraAsync(msg.ClienteId);
                    var cliente = await db.Clientes.FindAsync([msg.ClienteId], ct);
                    await emailSvc.EnviarPedidoRecibidoAsync(msg.ClienteId, ct);

                    consumer.Commit(cr);
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _log.LogWarning(ex, "Kafka desconectado – reintento en 5 s");
                await Task.Delay(5000, ct);
            }
        }
    }
}

