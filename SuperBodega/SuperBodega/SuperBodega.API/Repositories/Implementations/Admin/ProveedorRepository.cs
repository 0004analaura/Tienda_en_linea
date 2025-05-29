using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Repositories.Implementations.Admin;

public class ProveedorRepository : IProveedorRepository
{
    private readonly SuperBodegaContext _context;
    
    public ProveedorRepository(SuperBodegaContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Proveedor>> GetAllAsync()
    {
        return await _context.Proveedores.ToListAsync();
    }
    
    public async Task<Proveedor> GetByIdAsync(int id)
    {
        return await _context.Proveedores.FindAsync(id);
    }
    
    public async Task<Proveedor> AddAsync(Proveedor proveedor)
    {
        await _context.Proveedores.AddAsync(proveedor);
        await _context.SaveChangesAsync();
        return proveedor;
    }
    
    public async Task<Proveedor> UpdateAsync(Proveedor proveedor)
    {
        _context.Entry(proveedor).State = EntityState.Modified;
        _context.Entry(proveedor).Property(p => p.FechaDeRegistro).IsModified = false; // No modificar la fecha de registro
        await _context.SaveChangesAsync();
        return proveedor;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null) return false;

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();
        return true;
    }
    
}