using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Razor;
using SuperBodega.API.Data;
using SuperBodega.API.Services;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;
using SuperBodega.API.Repositories.Implementations.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;
using SuperBodega.API.Services.Admin;


var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de Kestrel para escuchar en el puerto 8080
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);  // Escucha en todas las interfaces en el puerto 8080
});

// Configuraci�n de vistas y Razor
builder.Services.AddControllersWithViews();

builder.Services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
{
    var viewsPath = Path.Combine(AppContext.BaseDirectory, "Views");
    if (!Directory.Exists(viewsPath))
    {
        viewsPath = Path.Combine(Directory.GetCurrentDirectory(), "Views");
    }
    options.FileProviders.Clear();
    options.FileProviders.Add(new PhysicalFileProvider(viewsPath));
});

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/Dashboard/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/CategoriaView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/ProductoView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/ProveedorView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/Ecommerce/Productos/{0}" + RazorViewEngine.ViewExtension);
});

// Configuraci�n de archivos est�ticos
builder.Services.Configure<StaticFileOptions>(options =>
{
    options.ServeUnknownFileTypes = true;
});

// Configuraci�n de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Obtener la cadena de conexi�n de la base de datos
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
Console.WriteLine($"Connection string: {connectionString}");

// Servicios y Repositorios para la base de datos
builder.Services.AddHostedService<DatabaseConnectionCheckService>();

builder.Services.AddDbContext<SuperBodegaContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

builder.Services.AddScoped<DatabaseInitializerService>();
builder.Services.AddScoped<CategoriaService>();

builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

builder.Services.AddScoped<ICompraProductoRepository, CompraProductoRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();

builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<ProveedorService>();
builder.Services.AddScoped<ClienteService>();

builder.Services.AddScoped<CompraProductoService>();
builder.Services.AddScoped<VentaService>();



// Configuraci�n de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configuraci�n de HealthChecks
builder.Services.AddHealthChecks();

builder.Services.AddControllers();

// Construcci�n de la aplicaci�n
var app = builder.Build();

// Inicializaci�n de la base de datos
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializerService>();
    await dbInitializer.InitializeAsync();
}

// Configuraci�n del entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configuraci�n del enrutamiento, archivos est�ticos, CORS y vistas Razor
app.UseRouting();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseHttpsRedirection();

// Mapeo de controladores API
app.MapControllers();

// Configuraci�n de HealthCheck
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var response = new
        {
            Status = report.Status.ToString(),
            HealthChecks = report.Entries.Select(e => new
            {
                Component = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description
            }),
            TotalDuration = report.TotalDuration
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

// Ejecutar la aplicaci�n
app.Run();
