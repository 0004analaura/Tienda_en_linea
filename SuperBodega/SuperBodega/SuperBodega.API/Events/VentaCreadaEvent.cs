namespace SuperBodega.API.Events;
public record VentaLineaEvent(int ProductoId, int Cantidad, decimal PrecioUnitario);
public record VentaCreadaEvent(int VentaId, DateTime Fecha, IEnumerable<VentaLineaEvent> Lineas);
