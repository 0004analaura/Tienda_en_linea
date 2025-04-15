using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.DTOs.Admin;

public class ProveedorDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaDeRegistro { get; set; }
}

public class CreateProveedorDTO
{
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre del proveedor no puede exceder los 100 caracteres")]
    public string Nombre { get; set; }
    
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(50, ErrorMessage = "El email no puede exceder los 50 caracteres")]
    public string Email { get; set; }
    
    [Phone(ErrorMessage = "El teléfono no es válido")]
    [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
    public string Telefono { get; set; }
    
    [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
    public string Direccion { get; set; }
    
    public bool Estado { get; set; }
    
    [Required(ErrorMessage = "La fecha de registro es obligatoria")]
    [DataType(DataType.Date, ErrorMessage = "La fecha de registro no es válida")]
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

public class UpdateProveedorDTO
{
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre del proveedor no puede exceder los 100 caracteres")]
    public string Nombre { get; set; }
    
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(50, ErrorMessage = "El email no puede exceder los 50 caracteres")]
    public string Email { get; set; }
    
    [Phone(ErrorMessage = "El teléfono no es válido")]
    [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
    public string Telefono { get; set; }
    
    [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
    public string Direccion { get; set; }
    
    public bool Estado { get; set; }
}