using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Services.Admin;

public class CompraProductoService
{
    private readonly SuperBodegaContext _context;
    private readonly ProductoService _productoService;

    public CompraProductoService(SuperBodegaContext context, ProductoService productoService)
    {
        _context = context;
        _productoService = productoService;
    }

    // Obtener todas las compras de productos
    public async Task<IEnumerable<CompraProductoDTO>> GetAllAsync()
    {
        var compraProductos = await _context.CompraProductos
            .Include(cp => cp.Producto)
            .Include(cp => cp.Proveedor)
            .ToListAsync();

        return compraProductos.Select(cp => new CompraProductoDTO
        {
            Id = cp.Id,
            CompraId = cp.CompraId,
            ProductoId = cp.ProductoId,
            NombreProducto = cp.Producto?.Nombre ?? "Producto no disponible",
            ProveedorId = cp.ProveedorId,
            NombreProveedor = cp.Proveedor?.Nombre ?? "Proveedor no disponible",
            Cantidad = cp.Cantidad,
            PrecioUnitario = cp.PrecioUnitario,
            PrecioVenta = cp.PrecioVenta,
            FechaDeCompra = cp.FechaDeCompra,
            // Total is calculated via the property
        });
    }

    // Obtener todos los productos de una compra
    public async Task<IEnumerable<CompraProducto>> GetProductosByCompraIdAsync(int compraId)
    {
        return await _context.CompraProductos
            .Where(cp => cp.CompraId == compraId)
            .Include(cp => cp.Producto)
            .ToListAsync();
    }

    // Obtener un producto específico por su Id
    public async Task<CompraProducto> GetByIdAsync(int id)
    {
        return await _context.CompraProductos
            .Include(cp => cp.Producto)
            .FirstOrDefaultAsync(cp => cp.Id == id);
    }

    // Crear un nuevo producto en la compra
    public async Task<CompraProducto> CreateAsync(CreateCompraProductoDTO dto)
    {
        var nuevo = new CompraProducto
        {
            ProductoId = dto.ProductoId,
            ProveedorId = dto.ProveedorId,
            Cantidad = dto.Cantidad,
            PrecioUnitario = dto.PrecioUnitario,
            PrecioVenta = dto.PrecioVenta,
            FechaDeCompra = dto.FechaDeCompra,
            Total = dto.Cantidad * dto.PrecioUnitario
        };

        // Get the execution strategy from the context
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Validate product existence
                    var producto = await _context.Productos.FindAsync(dto.ProductoId);
                    if (producto == null)
                        throw new ArgumentException("El producto con el ID especificado no existe.");

                    // Validate provider existence
                    var proveedor = await _context.Proveedores.FindAsync(dto.ProveedorId);
                    if (proveedor == null)
                        throw new ArgumentException("El proveedor con el ID especificado no existe.");

                    _context.CompraProductos.Add(nuevo);
                    await _context.SaveChangesAsync();

                    // Update product stock and prices
                    producto.Stock += dto.Cantidad;
                    producto.PrecioDeCompra = dto.PrecioUnitario;
                    producto.PrecioDeVenta = dto.PrecioVenta;
                    producto.ProveedorId = dto.ProveedorId;
                    _context.Productos.Update(producto);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        });

        return nuevo;
    }

    // Actualizar un producto en una compra
    public async Task<CompraProducto> UpdateAsync(int id, UpdateCompraProductoDTO dto)
    {
        var cp = await _context.CompraProductos.FindAsync(id);
        if (cp == null) return null;

        // Initial validations outside transaction
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null)
            throw new ArgumentException("El producto con el ID especificado no existe.");

        var diferenciaCantidad = dto.Cantidad - cp.Cantidad;
        var viejoTotal = cp.Total;
        var nuevoTotal = dto.Cantidad * dto.PrecioUnitario;
        var diferenciaTotalCompra = nuevoTotal - viejoTotal;

        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Update operations within transaction
                    cp.Cantidad = dto.Cantidad;
                    cp.PrecioUnitario = dto.PrecioUnitario;
                    cp.Total = dto.Cantidad * dto.PrecioUnitario;
                    _context.CompraProductos.Update(cp);

                    producto.Stock += diferenciaCantidad;
                    producto.PrecioDeCompra = dto.PrecioUnitario;
                    producto.PrecioDeVenta = dto.PrecioVenta;
                    _context.Productos.Update(producto);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        });

        return cp;
    }

    // Eliminar un producto de una compra
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var cp = await _context.CompraProductos
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cp == null) return false;

            var producto = cp.Producto;
            if (producto == null)
            {
                producto = await _context.Productos.FindAsync(cp.ProductoId);
                if (producto == null) return false;
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            var success = false;

            await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Update product stock
                        producto.Stock -= cp.Cantidad;
                        if (producto.Stock < 0) producto.Stock = 0;
                        _context.Productos.Update(producto);

                        // Remove the purchase product entry
                        _context.CompraProductos.Remove(cp);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting purchase product: {ex.Message}");
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            });

            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DeleteAsync: {ex.Message}");
            return false;
        }
    }
}
