using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Services.Admin;

public class CategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriaService
    (ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IEnumerable<CategoriaDTO>> GetAllCategoriasAsync()
    {
        var categorias = await _categoriaRepository.GetAllAsync();
        // Aquí puedes mapear las entidades a DTOs
        return categorias.Select(c => new CategoriaDTO
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion,
            Estado = c.Estado,
            FechaDeRegistro = c.FechaDeRegistro
        });
    }

    public async Task<CategoriaDTO> GetCategoriaByIdAsync(int id)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id);
        if (categoria == null)
        {
            return null;
        }
        return new CategoriaDTO
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado,
            FechaDeRegistro = categoria.FechaDeRegistro
        };
    }

    public async Task<CategoriaDTO> CreateCategoriaAsync(CreateCategoriaDTO categoriaDto)
    {
        var categoria = new Categoria
        {
            Nombre = categoriaDto.Nombre,
            Descripcion = categoriaDto.Descripcion,
            Estado = categoriaDto.Estado,
            FechaDeRegistro = categoriaDto.FechaDeRegistro
        };
        var newCategoria = await _categoriaRepository.AddAsync(categoria);
        return new CategoriaDTO
        {
            Id = newCategoria.Id,
            Nombre = newCategoria.Nombre,
            Descripcion = newCategoria.Descripcion,
            Estado = newCategoria.Estado,
            FechaDeRegistro = newCategoria.FechaDeRegistro
        };
    }

    public async Task<CategoriaDTO> UpdateCategoriaAsync(int id, UpdateCategoriaDTO categoriaDto)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id);
        if (categoria == null)
        {
            return null;
        }
        categoria.Nombre = categoriaDto.Nombre;
        categoria.Descripcion = categoriaDto.Descripcion;
        categoria.Estado = categoriaDto.Estado;
        var updatedCategoria = await _categoriaRepository.UpdateAsync(categoria);
        return new CategoriaDTO
        {
            Id = updatedCategoria.Id,
            Nombre = updatedCategoria.Nombre,
            Descripcion = updatedCategoria.Descripcion,
            Estado = updatedCategoria.Estado,
            FechaDeRegistro = updatedCategoria.FechaDeRegistro
        };
    }

    public async Task<bool> DeleteCategoriaAsync(int id)
    {
        return await _categoriaRepository.DeleteAsync(id);
    }
}