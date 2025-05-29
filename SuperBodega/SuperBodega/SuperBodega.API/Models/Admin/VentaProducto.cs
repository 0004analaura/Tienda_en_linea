using System;
using SuperBodega.API.Models.Admin;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin
{
    /// <summary>
    /// Clase que representa una venta del producto en la tienda.
    /// </summary>
    public class VentaProducto
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public Venta Venta { get; set; }

        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        public int Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }

        public decimal? Total => Cantidad * PrecioUnitario;
    }
}
