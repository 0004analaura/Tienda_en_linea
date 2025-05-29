using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin;

/// <summary>
/// Clase que representa un producto en la tienda.
/// </summary>
public class Producto
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Codigo { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [StringLength(200)]
    public string Descripcion { get; set; }

    [Required]
    public int CategoriaId { get; set; }

    [ForeignKey("CategoriaId")]
    public Categoria Categoria { get; set; }

    public int ProveedorId { get; set; }

    [Required]
    public int Stock { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioDeCompra { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioDeVenta { get; set; } = 0;

    public bool Estado { get; set; }

    public string? ImagenUrl { get; set; }

    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}