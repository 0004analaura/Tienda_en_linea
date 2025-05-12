using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SuperBodega.API.Models;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Ecommerce;

namespace SuperBodega.API.Data
{
    public class SuperBodegaContext : DbContext
    {
        // Bandera estática para asegurar que la creación de la base de datos se haga una sola vez por aplicación
        private static bool _databaseInitialized = false;
        private static readonly object _lockObject = new object();
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<CompraProducto> CompraProductos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaProducto> VentaProductos { get; set; }
        
        public DbSet<Carrito> Carritos { get; set; }

        public SuperBodegaContext(DbContextOptions<SuperBodegaContext> options) : base(options)
        {
            // Solo intentar crear la base de datos si aún no está inicializada
            if (!_databaseInitialized)
            {
                // Usar un lock para asegurar que solo un hilo intente inicializar la base de datos
                lock (_lockObject)
                {
                    if (!_databaseInitialized)
                    {
                        try
                        {
                            var dbCreator = Database.GetService<IRelationalDatabaseCreator>() as RelationalDatabaseCreator;
                            if (dbCreator != null)
                            {
                                if (!dbCreator.CanConnect())
                                {
                                    dbCreator.Create();
                                    Console.WriteLine("Base de datos creada.");
                                }

                                if (!dbCreator.HasTables())
                                {
                                    dbCreator.CreateTables();
                                    Console.WriteLine("Tablas creadas en la base de datos.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the exception
                            Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
                        }
                        finally
                        {
                            _databaseInitialized = true;
                        }
                    }
                }
            }
        }
        public DbSet<SuperBodega.API.DTOs.Admin.ProductoDTO> ProductoDTO { get; set; } = default!;
    }
}