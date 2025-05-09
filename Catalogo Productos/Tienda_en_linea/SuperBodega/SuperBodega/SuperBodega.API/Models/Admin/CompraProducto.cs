using System;
using SuperBodega.API.Models.Admin;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin

{
    public class CompraProducto
    {
        public int Id { get; set; }
        public int CompraId { get; set; }
        public int ProductoId { get; set; }
        public int ProveedorId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioVenta { get; set; }
        public DateTime FechaDeCompra { get; set; }
        public decimal Total { get; set; }

        public Producto Producto { get; set; }
        public Proveedor Proveedor { get; set; }
    }
}