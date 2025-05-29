namespace SuperBodega.API.Models.Admin
{
    /// <summary>
    /// Enumeración que representa los posibles estados de una venta.
    /// </summary>
    public enum EstadoVenta
    {
        Recibido = 1,   // generado al crear la venta (checkout)
        Despachado = 2,   // se marca cuando sale de bodega
        Entregado = 3    // se confirma al llegar al cliente
    }
}