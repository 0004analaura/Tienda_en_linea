using System;
using System.ComponentModel.DataAnnotations;
using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la venta.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class VentaDTO
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; }
    public List<VentaProductoDTO> Productos { get; set; }
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la vista de una venta.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class VentaViewDTO
{
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public decimal? Total { get; set; }
        public EstadoVenta Estado { get; set; }

        public List<VentaProducto> Productos { get; set; } = new();
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la creacion de una venta.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class CreateVentaDTO
{
    [Required] public DateTime Fecha { get; set; }
    [Required] public int ClienteId { get; set; }
    [Required] public List<CreateVentaProductoDTO> Productos { get; set; }
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la actualizacion de una venta.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class UpdateVentaDTO
{
    [Required] public DateTime Fecha { get; set; }
    [Required] public int ClienteId { get; set; }
    [Required] public List<CreateVentaProductoDTO> Productos { get; set; }
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la creacion de una venta por producto.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class CreateVentaProductoDTO
{
    [Required] public int ProductoId { get; set; }
    [Required][Range(1, int.MaxValue)] public int Cantidad { get; set; }
    [Required][Range(0.01, double.MaxValue)] public decimal PrecioUnitario { get; set; }
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la actualizacion del estado de la venta.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class UpdateEstadoVentaDTO
{
    [Required] public string Estado { get; set; }
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la venta de un producto.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class VentaProductoDTO
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
}
