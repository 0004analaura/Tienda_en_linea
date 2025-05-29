namespace SuperBodega.API.Models.Admin;

/// <summary>
/// Clase que representa un proveedor en la tienda.
/// </summary>
public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}