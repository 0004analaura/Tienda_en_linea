using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Repositories.Implementations.Admin;

public class ClienteRepository : IClienteRepository
{
    private readonly SuperBodegaContext _context;
    
    public ClienteRepository(SuperBodegaContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Cliente>> GetAllAsync()
    {
        return await _context.Clientes.ToListAsync();
    }
    
    public async Task<Cliente> GetByIdAsync(int id)
    {
        return await _context.Clientes.FindAsync(id);
    }
    
    public async Task<Cliente> AddAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }
    
    public async Task<Cliente> UpdateAsync(Cliente cliente)
    {
        _context.Entry(cliente).State = EntityState.Modified;
        _context.Entry(cliente).Property(c => c.FechaDeRegistro).IsModified = false; // No modificar la fecha de registro
        await _context.SaveChangesAsync();
        return cliente;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return false;

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return true;
    }
}