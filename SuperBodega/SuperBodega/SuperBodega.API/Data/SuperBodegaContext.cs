using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SuperBodega.API.Models;
using SuperBodega.API.Models.Admin;

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
                            
                            // Marcar como inicializada después de la creación exitosa
                            _databaseInitialized = true;
                            Console.WriteLine("Base de datos inicializada correctamente.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al inicializar base de datos: {ex.Message}");
                            // No marcamos como inicializada para que se intente nuevamente en el próximo reinicio
                        }
                    }
                }
            }
        }
    }
}