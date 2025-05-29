namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la categoría.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class CategoriaDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la creacion de una categoría.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class CreateCategoriaDTO
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Clase que representa un DTO (Data Transfer Object) para la actualizacion de una categoría.
/// Se utiliza para transferir datos entre la capa de presentación y la capa de negocio.
/// </summary>
public class UpdateCategoriaDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public bool Estado { get; set; }
}