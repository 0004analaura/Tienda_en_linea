namespace SuperBodega.API.Models.Admin;

/// <summary>
/// Clase que representa una categoría de productos en la tienda.
/// </summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}