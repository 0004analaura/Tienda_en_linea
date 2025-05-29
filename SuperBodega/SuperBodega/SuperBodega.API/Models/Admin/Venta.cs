using System;
using SuperBodega.API.Models.Admin;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin
{
    /// <summary>
    /// Clase que representa una venta en la tienda.
    /// </summary>
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public decimal? Total { get; set; }
        public EstadoVenta Estado { get; set; }

        public List<VentaProducto> Productos { get; set; } = new();
    }
}
