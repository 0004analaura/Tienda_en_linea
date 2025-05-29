namespace SuperBodega.API.DTOs.Admin
{
    /// <summary>
    /// Clase que representa un DTO (Data Transfer Object) para los reportes de las ventas por periodo.
    /// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
    /// </summary>
    public class ReporteVentaPeriodoDTO
    {
        public DateTime Fecha { get; set; }          // Día (o mes, según filtro)
        public int TotalTransacciones { get; set; }
        public decimal MontoTotal { get; set; }
    }

    /// <summary>
    /// Clase que representa un DTO (Data Transfer Object) para los reportes de las ventas por producto.
    /// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
    /// </summary>
    public class ReporteVentaProductoDTO
    {
        public int ProductoId { get; set; }
        public string Producto { get; set; } = default!;
        public int CantidadVendida { get; set; }
        public decimal MontoTotal { get; set; }
    }

    /// <summary>
    /// Clase que representa un DTO (Data Transfer Object) para los reportes de las ventas por cliente.
    /// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
    /// </summary>
    public class ReporteVentaClienteDTO
    {
        public int ClienteId { get; set; }
        public string Cliente { get; set; } = default!;
        public int TotalTransacciones { get; set; }
        public decimal MontoTotal { get; set; }
    }

    /// <summary>
    /// Clase que representa un DTO (Data Transfer Object) para los reportes de las ventas por proveedor.
    /// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
    /// </summary>
    public class ReporteVentaProveedorDTO
    {
        public int ProveedorId { get; set; }
        public string Proveedor { get; set; } = default!;
        public int CantidadProductosVendidos { get; set; }
        public decimal MontoTotal { get; set; }
    }

}
