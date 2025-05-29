using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Infrastructure.Messaging;
using SuperBodega.API.DTOs.Ecommerce;

namespace SuperBodega.API.Controllers.Ecommerce;
/// <summary>
/// Controlador para gestionar carritos de compra v2 en el sistema
/// </summary>
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CarritoV2Controller : ControllerBase
{
    private readonly IKafkaProducerService _producer;
    public CarritoV2Controller(IKafkaProducerService producer) => _producer = producer;

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CarritoV2DTO dto)
    {
        var evt = new CarritoCheckoutMessage(dto.ClienteId, DateTime.UtcNow,
                   dto.Items.Select(i => new Linea(i.ProductoId, i.Cantidad)));
        await _producer.PublishAsync("compras", evt);
        return Accepted();
    }
}