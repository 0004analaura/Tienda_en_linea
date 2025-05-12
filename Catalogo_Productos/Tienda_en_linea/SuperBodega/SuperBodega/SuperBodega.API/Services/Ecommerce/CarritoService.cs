using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.DTOs.Ecommerce;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Models.Ecommerce;

namespace SuperBodega.API.Services.Ecommerce;

public class CarritoService
{
    private readonly SuperBodegaContext _context;

    public CarritoService(SuperBodegaContext context)
    {
        _context = context;
    }

    public async Task<List<CarritoDTO>> GetAllCarritosAsync()
    {
        var carritos = await _context.Carritos
            .Include(c => c.Cliente)
            .Include(c => c.Items)
            .ThenInclude(i => i.Producto)
            .ToListAsync();

        return carritos.Select(c => MapToCarritoDTO(c)).ToList();
    }

    public async Task<CarritoDTO> GetCarritoByClienteIdAsync(int clienteId)
    {
        var carrito = await _context.Carritos
            .Include(c => c.Cliente)
            .Include(c => c.Items)
            .ThenInclude(i => i.Producto)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrito == null)
        {
            // Si no existe un carrito para este cliente, devolver un DTO vacío
            return new CarritoDTO 
            { 
                ClienteId = clienteId,
                ClienteNombre = await _context.Clientes
                    .Where(c => c.Id == clienteId)
                    .Select(c => $"{c.Nombre} {c.Apellido}")
                    .FirstOrDefaultAsync() ?? "Cliente no encontrado"
            };
        }

        return MapToCarritoDTO(carrito);
    }

    public async Task<CarritoDTO> AgregarAlCarritoAsync(AgregarAlCarritoDTO dto)
    {
        // Validar que el producto exista y tenga stock
        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null)
        {
            throw new Exception("El producto no existe");
        }

        if (producto.Stock < dto.Cantidad)
        {
            throw new Exception("No hay suficiente stock disponible");
        }

        // Buscar o crear el carrito para el cliente
        var carrito = await _context.Carritos
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.ClienteId == dto.ClienteId);

        if (carrito == null)
        {
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
            {
                throw new Exception("El cliente no existe");
            }

            carrito = new Carrito
            {
                ClienteId = dto.ClienteId,
                Cliente = cliente,
                FechaCreacion = DateTime.Now
            };

            _context.Carritos.Add(carrito);
        }

        // Verificar si el producto ya está en el carrito
        var itemExistente = carrito.Items.FirstOrDefault(i => i.ProductoId == dto.ProductoId);
        if (itemExistente != null)
        {
            // Actualizar cantidad
            itemExistente.Cantidad += dto.Cantidad;
        }
        else
        {
            // Agregar nuevo item
            carrito.Items.Add(new CarritoItem
            {
                ProductoId = dto.ProductoId,
                Producto = producto,
                Cantidad = dto.Cantidad,
                PrecioUnitario = producto.PrecioDeVenta
            });
        }

        await _context.SaveChangesAsync();

        return await GetCarritoByClienteIdAsync(dto.ClienteId);
    }

    public async Task<CarritoDTO> ActualizarCantidadItemAsync(ActualizarCarritoItemDTO dto)
    {
        var item = await _context.Set<CarritoItem>()
            .Include(i => i.Carrito)
            .Include(i => i.Producto)
            .FirstOrDefaultAsync(i => i.Id == dto.CarritoItemId);

        if (item == null)
        {
            throw new Exception("El item del carrito no existe");
        }

        // Validar stock
        if (dto.Cantidad > 0 && item.Producto.Stock < dto.Cantidad)
        {
            throw new Exception("No hay suficiente stock disponible");
        }

        if (dto.Cantidad <= 0)
        {
            // Si la cantidad es 0 o menos, eliminar el item
            _context.Remove(item);
        }
        else
        {
            // Actualizar cantidad
            item.Cantidad = dto.Cantidad;
        }

        await _context.SaveChangesAsync();

        return await GetCarritoByClienteIdAsync(item.Carrito.ClienteId);
    }

    public async Task<CarritoDTO> EliminarItemDelCarritoAsync(int itemId)
    {
        var item = await _context.Set<CarritoItem>()
            .Include(i => i.Carrito)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
        {
            throw new Exception("El item del carrito no existe");
        }

        int clienteId = item.Carrito.ClienteId;

        _context.Remove(item);
        await _context.SaveChangesAsync();

        return await GetCarritoByClienteIdAsync(clienteId);
    }

    public async Task<CarritoDTO> GetCarritoAsync()
    {
        // Este método se usará cuando no sabemos qué cliente está logueado
        // Para la implementación actual, vamos a devolver un carrito vacío
        // En una app real, esto vendría del servicio de autenticación
        return new CarritoDTO
        {
            Items = new List<CarritoItemDTO>(),
            Total = 0
        };
    }

    public async Task VaciarCarritoAsync(int clienteId)
    {
        var carrito = await _context.Carritos
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrito != null)
        {
            // Eliminar todos los items
            _context.RemoveRange(carrito.Items);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> RealizarCompraAsync(int clienteId)
    {
        // Use CreateExecutionStrategy to create a strategy that can safely retry the transaction
        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            // Start transaction inside the execution strategy
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Obtener el carrito del cliente
                var carrito = await _context.Carritos
                    .Include(c => c.Cliente)
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

                if (carrito == null || !carrito.Items.Any())
                {
                    throw new Exception("El carrito está vacío o no existe");
                }

                // Verificar stock disponible
                foreach (var item in carrito.Items)
                {
                    if (item.Cantidad > item.Producto.Stock)
                    {
                        throw new Exception($"No hay suficiente stock para el producto {item.Producto.Nombre}");
                    }
                }

                // Crear una nueva venta
                var venta = new Venta
                {
                    ClienteId = clienteId,
                    Fecha = DateTime.Now,
                    Total = carrito.Total ?? 0,
                    Estado = EstadoVenta.Completada
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                // Agregar productos a la venta y actualizar stock
                foreach (var item in carrito.Items)
                {
                    var ventaProducto = new VentaProducto
                    {
                        VentaId = venta.Id,
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario
                    };

                    _context.VentaProductos.Add(ventaProducto);

                    // Actualizar el stock del producto
                    item.Producto.Stock -= item.Cantidad;
                    _context.Update(item.Producto);
                }

                // Guardar cambios de productos de venta y actualización de stock
                await _context.SaveChangesAsync();

                // Vaciar el carrito
                _context.RemoveRange(carrito.Items);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private CarritoDTO MapToCarritoDTO(Carrito carrito)
    {
        return new CarritoDTO
        {
            Id = carrito.Id,
            ClienteId = carrito.ClienteId,
            ClienteNombre = carrito.Cliente != null ? $"{carrito.Cliente.Nombre} {carrito.Cliente.Apellido}" : "",
            FechaCreacion = carrito.FechaCreacion,
            Items = carrito.Items.Select(i => new CarritoItemDTO
            {
                Id = i.Id,
                ProductoId = i.ProductoId,
                ProductoNombre = i.Producto?.Nombre ?? "Producto no disponible",
                ProductoImagenUrl = i.Producto?.ImagenUrl ?? "/images/productos/default_product.png",
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Subtotal = i.Cantidad * i.PrecioUnitario
            }).ToList(),
            Total = carrito.Items.Sum(i => i.Cantidad * i.PrecioUnitario)
        };
    }
}