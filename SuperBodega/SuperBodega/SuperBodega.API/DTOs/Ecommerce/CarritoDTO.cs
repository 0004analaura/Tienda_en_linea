namespace SuperBodega.API.DTOs.Ecommerce;

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para el carrito de compras.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class CarritoDTO
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<CarritoItemDTO> Items { get; set; } = new List<CarritoItemDTO>();
    public decimal? Total { get; set; }
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para el item del carrito de compras.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class CarritoItemDTO
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; }
    public string ProductoImagenUrl { get; set; }
    public int Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public decimal? Subtotal { get; set; }
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para agregar al carrito de compras.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class AgregarAlCarritoDTO
{
    public int ClienteId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para actualizar el carrito de compras.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class ActualizarCarritoItemDTO
{
    public int CarritoItemId { get; set; }
    public int Cantidad { get; set; }
}