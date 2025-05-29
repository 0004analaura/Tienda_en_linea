using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.DTOs.Ecommerce;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Models.Ecommerce;

namespace SuperBodega.API.Services.Ecommerce;

public class CarritoService
{
    private readonly SuperBodegaContext _context;
    private readonly ILogger<CarritoService> _logger;

    public CarritoService(SuperBodegaContext context, ILogger<CarritoService> logger)
    {
        _context = context;
        _logger = logger;
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

    public async Task<int> RealizarCompraAsync(int clienteId)
    {
        _logger.LogInformation("Iniciando proceso de compra para cliente {ClienteId}", clienteId);

        // Usar la estrategia de ejecución de Entity Framework para manejar reintentos
        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                _logger.LogInformation("Buscando carrito para cliente {ClienteId}", clienteId);

                // 1. Obtener carrito con items y productos (con include para evitar lazy loading)
                var carrito = await _context.Carritos
                    .Include(c => c.Cliente)
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

                if (carrito == null || !carrito.Items.Any())
                {
                    _logger.LogWarning("Carrito vacío o inexistente para cliente {ClienteId}", clienteId);
                    throw new InvalidOperationException("El carrito está vacío o no existe");
                }

                _logger.LogInformation("Carrito encontrado con {ItemCount} items", carrito.Items.Count);

                // 2. Validar stock de todos los productos antes de crear la venta
                var erroresStock = new List<string>();
                foreach (var item in carrito.Items)
                {
                    if (item.Producto == null)
                    {
                        erroresStock.Add($"Producto con ID {item.ProductoId} no encontrado");
                        continue;
                    }
                    
                    if (item.Producto.Stock < item.Cantidad)
                    {
                        erroresStock.Add($"Stock insuficiente para {item.Producto.Nombre}. Disponible: {item.Producto.Stock}, Requerido: {item.Cantidad}");
                    }
                }

                if (erroresStock.Any())
                {
                    _logger.LogWarning("Errores de stock: {Errores}", string.Join(", ", erroresStock));
                    throw new InvalidOperationException($"Errores de stock: {string.Join(", ", erroresStock)}");
                }

                _logger.LogInformation("Validación de stock completada");

                // 3. Crear la venta
                var venta = new Venta
                {
                    ClienteId = clienteId,
                    Fecha = DateTime.Now,
                    Estado = EstadoVenta.Recibido,
                    Total = 0 // Se calculará después
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Venta {VentaId} creada", venta.Id);

                // 4. Procesar cada item del carrito
                decimal? totalVenta = 0;
                var ventaProductos = new List<VentaProducto>();

                foreach (var item in carrito.Items)
                {
                    // Crear producto de venta
                    var ventaProducto = new VentaProducto
                    {
                        VentaId = venta.Id,
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario ?? item.Producto.PrecioDeVenta
                    };

                    ventaProductos.Add(ventaProducto);
                    totalVenta += (ventaProducto.PrecioUnitario * ventaProducto.Cantidad);

                    // Actualizar stock
                    item.Producto.Stock -= item.Cantidad;
                    _context.Productos.Update(item.Producto);

                    _logger.LogInformation("Procesado item: {ProductoNombre} x{Cantidad} = Q{Subtotal}", 
                        item.Producto.Nombre, item.Cantidad, ventaProducto.PrecioUnitario * ventaProducto.Cantidad);
                }

                // 5. Agregar todos los productos de venta
                _context.VentaProductos.AddRange(ventaProductos);

                // 6. Actualizar total de la venta
                venta.Total = totalVenta;
                _context.Ventas.Update(venta);

                _logger.LogInformation("Total de venta: Q{Total}", totalVenta);

                // 7. Vaciar el carrito
                _context.RemoveRange(carrito.Items);
                _context.Carritos.Remove(carrito);

                _logger.LogInformation("Carrito vaciado para cliente {ClienteId}", clienteId);

                // 8. Guardar todos los cambios
                await _context.SaveChangesAsync();
                
                // 9. Confirmar la transacción
                await transaction.CommitAsync();

                _logger.LogInformation("Compra completada exitosamente. Venta ID: {VentaId}, Total: Q{Total}", 
                    venta.Id, venta.Total);

                return venta.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la compra para cliente {ClienteId}", clienteId);
                
                // Si algo falla, hacer rollback
                await transaction.RollbackAsync();
                throw; // Re-lanzar la excepción para que la estrategia pueda reintentar si es apropiado
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