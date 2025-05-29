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
using SuperBodega.API.Services.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Infrastructure.Messaging;
using SuperBodega.API.Infrastructure.Email;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Routing;

var builder = WebApplication.CreateBuilder(args);

// Configuracion de Kestrel para escuchar en el puerto 8080
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);  // Escucha en todas las interfaces en el puerto 8080
});

// Configuracion de vistas y Razor
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
    options.ViewLocationFormats.Add("/Views/Ecommerce/CarritoView/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/VentasView/{0}" + RazorViewEngine.ViewExtension);
});

// Configuracion de archivos estaticos
builder.Services.Configure<StaticFileOptions>(options =>
{
    options.ServeUnknownFileTypes = true;
});

// Configuracion de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});

// Configuracion de Swagger con filtros
builder.Services.AddSwaggerGen(opt =>
{
    var provider = builder.Services.BuildServiceProvider()
                                   .GetRequiredService<IApiVersionDescriptionProvider>();
    
    foreach (var desc in provider.ApiVersionDescriptions)
    {
        opt.SwaggerDoc(desc.GroupName,
            new Microsoft.OpenApi.Models.OpenApiInfo 
            { 
                Title = "SuperBodega API", 
                Version = desc.GroupName,
                Description = "API para el sistema SuperBodega"
            });
    }

    // Filtrar solo controladores API (excluir MVC controllers)
    opt.DocInclusionPredicate((docName, apiDesc) =>
    {
        // Incluir solo acciones que tienen ApiController o están en rutas api/*
        var controller = apiDesc.ActionDescriptor.RouteValues["controller"];
        var action = apiDesc.ActionDescriptor.RouteValues["action"];
        
        // Excluir controladores MVC específicos
        var excludeControllers = new[] { "Dashboard", "Home" };
        
        return !excludeControllers.Contains(controller, StringComparer.OrdinalIgnoreCase);
    });
});

// Obtener la cadena de conexion de la base de datos
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
builder.Services.AddScoped<VentaEstadoService>();
builder.Services.AddScoped<CarritoService>();
builder.Services.AddScoped<ReporteService>();

// Configuracion de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configuracion de HealthChecks
builder.Services.AddHealthChecks();

builder.Services.AddControllers();


// Validar configuración de email
var emailConfig = builder.Configuration.GetSection("Email");
if (string.IsNullOrEmpty(builder.Configuration["EMAIL_SMTP_PASSWORD"]))
{
    builder.Services.AddLogging(config => config.AddConsole());
    var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Program>>();
    logger?.LogWarning("EMAIL_SMTP_PASSWORD no está configurado. El servicio de email no funcionará correctamente.");
}

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerHostedService>();


// Construccion de la aplicacion
var app = builder.Build();


// Inicializacion de la base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SuperBodegaContext>();
    db.Database.Migrate();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializerService>();
    await dbInitializer.InitializeAsync();
}

// Configuracion del entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
            c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
    });
}

// Configuracion del enrutamiento, archivos estaticos, CORS y vistas Razor
app.UseRouting();
app.UseStaticFiles();

app.UseCors("AllowAll");

// Rutas específicas para dashboards
app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard/{action=Index}",
    defaults: new { controller = "DashboardView" });

// Rutas para áreas admin
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

// Rutas para ecommerce
app.MapControllerRoute(
    name: "ecommerce",
    pattern: "Ecommerce/{controller=Home}/{action=Index}/{id?}");

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseHttpsRedirection();

// Mapeo de controladores API
app.MapControllers();
// app.MapDefaultControllerRoute();

// Configuracion de HealthCheck
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

app.Logger.LogInformation("Rutas:");
foreach (var e in app.Services.GetRequiredService<EndpointDataSource>().Endpoints)
    app.Logger.LogInformation(e.DisplayName);


// Ejecutar la aplicacion
app.Run();
