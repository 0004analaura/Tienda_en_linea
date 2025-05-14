namespace SuperBodega.API.DTOs.Ecommerce;

public class CarritoDTO
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<CarritoItemDTO> Items { get; set; } = new List<CarritoItemDTO>();
    public decimal? Total { get; set; }
}

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

public class AgregarAlCarritoDTO
{
    public int ClienteId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class ActualizarCarritoItemDTO
{
    public int CarritoItemId { get; set; }
    public int Cantidad { get; set; }
}