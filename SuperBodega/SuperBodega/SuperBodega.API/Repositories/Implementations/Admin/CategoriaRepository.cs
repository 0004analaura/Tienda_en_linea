using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Services.Admin;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly SuperBodegaContext _context;

    public CategoriaRepository(SuperBodegaContext context)
    {
        this._context = context;
    }

    public async Task<IEnumerable<Categoria>> GetAllAsync()
    {
        return await _context.Categorias.ToListAsync();
    }

    public async Task<Categoria> GetByIdAsync(int id)
    {
        return await _context.Categorias.FindAsync(id);
    }

    public async Task<Categoria> AddAsync(Categoria categoria)
    {
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<Categoria> UpdateAsync(Categoria categoria)
    {
        _context.Entry(categoria).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
            return false;
        }
        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return true;
    }
}