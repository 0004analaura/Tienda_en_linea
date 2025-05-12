using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Repositories.Implementations.Admin;

public class ProductoRepository : IProductoRepository
{
    private readonly SuperBodegaContext _context;

    public ProductoRepository(SuperBodegaContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .ToListAsync();
    }

    public async Task<Producto> GetByIdAsync(int id)
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId)
    {
        return await _context.Productos
            .Where(p => p.CategoriaId == categoriaId)
            .Include(p => p.Categoria)
            .ToListAsync();
    }

    public async Task<Producto> AddAsync(Producto producto)
    {
        // Verificar si el producto ya existe por código para evitar duplicados
        var existingProduct = await _context.Productos.FirstOrDefaultAsync(p => p.Codigo == producto.Codigo);
        if (existingProduct != null)
        {
            // Si ya existe, retornar el existente o lanzar una excepción
            throw new InvalidOperationException($"Ya existe un producto con el código {producto.Codigo}");
        }
        
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        
        // Cargar la categoría para asegurar que esté disponible en el objeto devuelto
        await _context.Entry(producto)
            .Reference(p => p.Categoria)
            .LoadAsync();
            
        return producto;
    }

    public async Task<Producto> UpdateAsync(Producto producto)
    {
        _context.Entry(producto).State = EntityState.Modified;
        _context.Entry(producto).Property(p => p.FechaDeRegistro).IsModified = false;
    
        try
        {
            await _context.SaveChangesAsync();
            return producto;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductoExistsAsync(producto.Id))
            {
                return null;
            }
    
            throw;
        }
    }

    private async Task<bool> ProductoExistsAsync(int id)
    {
        return await _context.Productos.AnyAsync(p => p.Id == id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return false;
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStockAsync(int productoId, int cantidad)
    {
        var producto = await _context.Productos.FindAsync(productoId);
        if (producto == null)
        {
            return false;
        }

        producto.Stock += cantidad;
        if (producto.Stock <= 0)
        {
            return false;
        }

        var result = await UpdateAsync(producto);
        return result != null;    
    }
}