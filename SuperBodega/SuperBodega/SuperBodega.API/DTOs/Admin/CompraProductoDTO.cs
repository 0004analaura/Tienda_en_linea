using System;
using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.DTOs.Admin
{
    /// <summary>
    /// Clase que representa un DTO (Data Transfer Object) para la compra de un producto.
    /// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
    /// </summary>
    public class CompraProductoDTO
    {
        public int Id { get; set; }

        public int CompraId { get; set; }

        [Required(ErrorMessage = "El ID del producto es obligatorio.")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El ID del proveedor es obligatorio.")]
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        public string NombreProducto { get; set; }

        [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
        public string NombreProveedor { get; set; }

        [Required(ErrorMessage = "La cantidad comprada es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un n�mero positivo.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor que cero.")]
        public decimal PrecioUnitario { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor que cero.")]
        public decimal PrecioVenta { get; set; }

        [Required(ErrorMessage = "La fecha de compra es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaDeCompra { get; set; }

        // Propiedad calculada
        public decimal Total => Cantidad * PrecioUnitario;
    }

    /// <summary>
    /// Clase que representa un DTO (Data Transfer Object) para la creacion de una compra de un producto.
    /// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
    /// </summary>
    public class CreateCompraProductoDTO
    {
        [Required(ErrorMessage = "El ID del producto es obligatorio.")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El ID del proveedor es obligatorio.")]
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "La cantidad comprada es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un n�mero positivo.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor que cero.")]
        public decimal PrecioUnitario { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor que cero.")]
        public decimal PrecioVenta { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaDeCompra { get; set; }

        // Propiedad calculada
        public decimal Total => Cantidad * PrecioUnitario;
    }

    /// <summary>
    /// Clase que representa un DTO (Data Transfer Object) para la actualizacion de una compra de un producto.
    /// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
    /// </summary>
    public class UpdateCompraProductoDTO
    {
        public int CompraId { get; set; }

        [Required(ErrorMessage = "El ID del producto es obligatorio.")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad comprada es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un n�mero positivo.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor que cero.")]
        public decimal PrecioUnitario { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor que cero.")]
        public decimal PrecioVenta { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaDeCompra { get; set; }

        // Propiedad calculada
        public decimal Total => Cantidad * PrecioUnitario;
    }
}