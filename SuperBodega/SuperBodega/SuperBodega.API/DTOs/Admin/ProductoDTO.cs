using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para el producto.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class ProductoDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; }
    public int Stock { get; set; }
    public decimal? PrecioDeCompra { get; set; }
    public decimal? PrecioDeVenta { get; set; }
    public bool Estado { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTime FechaDeRegistro { get; set; }
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la creacion del producto.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class CreateProductoDTO
{
    [Required(ErrorMessage = "El codigo es obligatorio")]
    [StringLength(50, ErrorMessage = "El codigo no puede superar los 50 caracteres")]
    public string Codigo { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Nombre { get; set; }

    [StringLength(200, ErrorMessage = "La descripcion no puede superar los 200 caracteres")]
    public string Descripcion { get; set; }

    [Required(ErrorMessage = "La categoria es obligatoria")]
    public int CategoriaId { get; set; }

    public int Stock { get; set; }

    [Range(0, 999999.99, ErrorMessage = "El precio de compra debe estar entre 0 y 999999.99")]
    public decimal? PrecioDeCompra { get; set; } = 0;

    [Range(0, 999999.99, ErrorMessage = "El precio de venta debe estar entre 0 y 999999.99")]
    public decimal? PrecioDeVenta { get; set; } = 0;

    public bool Estado { get; set; }

    public string? ImagenUrl { get; set; }

    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la actualizacion del producto.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class UpdateProductoDTO
{
    [Required(ErrorMessage = "El código es obligatorio")]
    [StringLength(50, ErrorMessage = "El código no puede superar los 50 caracteres")]
    public string Codigo { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
    public string Nombre { get; set; }

    [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres")]
    public string Descripcion { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    public int CategoriaId { get; set; }

    [Range(0, 999999.99, ErrorMessage = "El precio de compra debe estar entre 0 y 999999.99")]
    public decimal? PrecioDeCompra { get; set; }

    [Range(0, 999999.99, ErrorMessage = "El precio de venta debe estar entre 0 y 999999.99")]
    public decimal? PrecioDeVenta { get; set; }

    public bool Estado { get; set; }

    public string? ImagenUrl { get; set; }
}