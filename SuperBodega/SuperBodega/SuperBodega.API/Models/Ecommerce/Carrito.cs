using System.ComponentModel.DataAnnotations.Schema;
using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Models.Ecommerce
{
    /// <summary>
    /// Clase que representa un carrito de compras en la tienda.
    /// Un carrito puede contener múltiples items, cada uno representando un producto y su cantidad.
    /// El total del carrito se calcula automáticamente sumando los subtotales de cada item.
    /// </summary>
    [Table("Carrito")]
    public class Carrito
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación a los items del carrito
        public List<CarritoItem> Items { get; set; } = new List<CarritoItem>();

        // Total del carrito (calculado)
        public decimal? Total => Items.Sum(i => i.Subtotal);
    }

    public class CarritoItem
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }
        public Carrito Carrito { get; set; }
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }

        // Subtotal (calculado)
        public decimal? Subtotal => Cantidad * PrecioUnitario;
    }

}
