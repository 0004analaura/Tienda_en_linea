using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers;

/// <summary>
/// Controlador para gestionar categorias en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class CategoriaController : ControllerBase
{
    private readonly CategoriaService _categoriaService;

    public CategoriaController(CategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _categoriaService.GetAllCategoriasAsync();
        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }
        return Ok(categoria);
    }

    [HttpPost ("Create")]
    public async Task<IActionResult> Create(CreateCategoriaDTO categoriaDto)
    {
        var newCategoria = await _categoriaService.CreateCategoriaAsync(categoriaDto);
        return CreatedAtAction(nameof(GetById), new { id = newCategoria.Id }, newCategoria);
    }

    [HttpPut("Edit/{id}")]
    public async Task<IActionResult> Update(int id, UpdateCategoriaDTO updateCategoriaDto)
    {
        var updatedCategoria = await _categoriaService.UpdateCategoriaAsync(id, updateCategoriaDto);
        if (updatedCategoria == null)
        {
            return NotFound();
        }
        return Ok(updatedCategoria);
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoriaService.DeleteCategoriaAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}