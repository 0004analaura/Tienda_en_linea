using System;
using System.ComponentModel.DataAnnotations;

namespace SuperBodega.API.DTOs.Admin;

public class CategoriaDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

public class CreateCategoriaDTO
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

public class UpdateCategoriaDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public bool Estado { get; set; }
}