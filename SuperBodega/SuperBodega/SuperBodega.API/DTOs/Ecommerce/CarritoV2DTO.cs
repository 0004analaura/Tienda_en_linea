namespace SuperBodega.API.DTOs.Ecommerce;

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para el carrito de compras version 2.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class CarritoV2DTO
{
    public int ClienteId { get; set; }
    public List<Linea> Items { get; set; } = new();
}

public record CarritoCheckoutMessage(int ClienteId,
                                 DateTime Fecha,
                                 IEnumerable<Linea> Items);
public record Linea(int ProductoId, int Cantidad);