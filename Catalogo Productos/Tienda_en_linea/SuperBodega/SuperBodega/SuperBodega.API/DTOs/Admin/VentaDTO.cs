using System;
using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.DTOs.Admin;

public class VentaDTO
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; }
    public List<VentaProductoDTO> Productos { get; set; }
}

public class CreateVentaDTO
{
    [Required] public DateTime Fecha { get; set; }
    [Required] public int ClienteId { get; set; }
    [Required] public List<CreateVentaProductoDTO> Productos { get; set; }
}

public class UpdateVentaDTO
{
    [Required] public DateTime Fecha { get; set; }
    [Required] public int ClienteId { get; set; }
    [Required] public List<CreateVentaProductoDTO> Productos { get; set; }
}

public class CreateVentaProductoDTO
{
    [Required] public int ProductoId { get; set; }
    [Required][Range(1, int.MaxValue)] public int Cantidad { get; set; }
    [Required][Range(0.01, double.MaxValue)] public decimal PrecioUnitario { get; set; }
}

public class UpdateEstadoVentaDTO
{
    [Required] public string Estado { get; set; }
}

public class VentaProductoDTO
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
}
