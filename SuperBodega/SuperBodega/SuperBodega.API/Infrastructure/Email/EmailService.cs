using System.Net;
using System.Net.Mail;
using SuperBodega.API.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Text;
using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Infrastructure.Email;

public interface IEmailService
{
    Task EnviarAsync(string to, string subject, string body, CancellationToken ct = default);
    Task EnviarPedidoRecibidoAsync(int clienteId, CancellationToken ct = default);
    Task EnviarActualizacionEstadoAsync(int ventaId, string nuevoEstado, CancellationToken ct = default);
}

public sealed class EmailService : IEmailService
{
    private readonly SuperBodegaContext _db;
    private readonly string _from;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpPassword;
    private readonly ILogger<EmailService> _logger;

    public EmailService(SuperBodegaContext db, IConfiguration cfg, ILogger<EmailService> logger)
    {
        _db = db;
        _logger = logger;
        _from = cfg["EMAIL_FROM"] ?? "andreaatellez904@gmail.com";
        _smtpServer = cfg["EMAIL_SMTP_SERVER"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(cfg["EMAIL_SMTP_PORT"] ?? "587");
        _smtpPassword = cfg["EMAIL_SMTP_PASSWORD"] ?? "";
        
        _logger.LogInformation("Email configurado - From: {From}, Server: {Server}, Port: {Port}", 
            _from, _smtpServer, _smtpPort);
    }

    private DateTime ConvertirFechaAGuatemala(DateTime fecha)
    {
        try 
        {
            var guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
            
            switch (fecha.Kind)
            {
                case DateTimeKind.Unspecified:
                    // Ya está en hora local de Guatemala, no convertir
                    return fecha;
                    
                case DateTimeKind.Utc:
                    // Convertir de UTC a Guatemala
                    return TimeZoneInfo.ConvertTimeFromUtc(fecha, guatemalaZone);
                    
                case DateTimeKind.Local:
                    // Convertir primero a UTC y luego a Guatemala
                    var utcTime = fecha.ToUniversalTime();
                    return TimeZoneInfo.ConvertTimeFromUtc(utcTime, guatemalaZone);
                    
                default:
                    return fecha;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al convertir fecha a zona horaria de Guatemala, usando fecha original");
            return fecha;
        }
    }

    public async Task EnviarAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_smtpPassword))
        {
            _logger.LogError("La contraseña SMTP no está configurada");
            throw new InvalidOperationException("La contraseña SMTP no está configurada");
        }

        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_from, _smtpPassword),
            EnableSsl = true,
            UseDefaultCredentials = false,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_from, "SuperBodega"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        try
        {
            _logger.LogInformation("Enviando email a {To} con asunto '{Subject}'", to, subject);
            await client.SendMailAsync(mailMessage, ct);
            _logger.LogInformation("Email enviado exitosamente a {To}", to);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "Error SMTP al enviar email a {To}: {Message}", to, ex.Message);
            throw new InvalidOperationException($"Error al enviar email: {ex.Message}", ex);
        }
    }

    public async Task EnviarPedidoRecibidoAsync(int clienteId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Iniciando envío de email de pedido recibido para cliente {ClienteId}", clienteId);
    
            var cliente = await _db.Clientes.FindAsync(new object?[] { clienteId }, ct);
            if (cliente is null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", clienteId);
                return;
            }
    
            if (string.IsNullOrWhiteSpace(cliente.Email))
            {
                _logger.LogWarning("Cliente {ClienteId} ({Nombre}) no tiene email configurado", clienteId, cliente.Nombre);
                return;
            }
    
            _logger.LogInformation("Cliente encontrado: {ClienteNombre} <{Email}>", cliente.Nombre, cliente.Email);
    
            // Obtener la última venta del cliente - USAR ID en lugar de Fecha para mejor precisión
            var ultimaVenta = await _db.Ventas
                .Include(v => v.Productos)
                .ThenInclude(vp => vp.Producto)
                .Where(v => v.ClienteId == clienteId)
                .OrderByDescending(v => v.Id)
                .FirstOrDefaultAsync(ct);
    
            if (ultimaVenta == null)
            {
                _logger.LogWarning("No se encontró ninguna venta para cliente {ClienteId}", clienteId);
                
                // Intentar buscar todas las ventas para debugging
                var todasLasVentas = await _db.Ventas
                    .Where(v => v.ClienteId == clienteId)
                    .Select(v => new { v.Id, v.Fecha })
                    .ToListAsync(ct);
                    
                _logger.LogInformation("Cliente {ClienteId} tiene {CantidadVentas} ventas en total: {VentasIds}", 
                    clienteId, todasLasVentas.Count, string.Join(", ", todasLasVentas.Select(v => v.Id)));
                
                return;
            }
    
            _logger.LogInformation("Venta encontrada: ID {VentaId}, Total: Q{Total}, Productos: {CantidadProductos}", 
                ultimaVenta.Id, ultimaVenta.Total, ultimaVenta.Productos.Count);
    
            var subject = $"SuperBodega – ¡Pedido #{ultimaVenta.Id} recibido!";
            var body = GenerarHtmlPedidoRecibido(cliente, ultimaVenta);
    
            _logger.LogInformation("Enviando email a {Email} con asunto: {Subject}", cliente.Email, subject);
    
            await EnviarAsync(cliente.Email, subject, body, ct);
            
            _logger.LogInformation("Email de pedido recibido enviado exitosamente para venta {VentaId}", ultimaVenta.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en EnviarPedidoRecibidoAsync para cliente {ClienteId}", clienteId);
            throw; // Re-lanzar para que el controller lo maneje
        }
    }

    public async Task EnviarActualizacionEstadoAsync(int ventaId, string nuevoEstado, CancellationToken ct = default)
    {
        var venta = await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Productos)
            .ThenInclude(vp => vp.Producto)
            .FirstOrDefaultAsync(v => v.Id == ventaId, ct);

        if (venta?.Cliente == null || string.IsNullOrWhiteSpace(venta.Cliente.Email)) return;

        var subject = $"SuperBodega – Actualización de tu pedido #{venta.Id}";
        var body = GenerarHtmlActualizacionEstado(venta, nuevoEstado);

        await EnviarAsync(venta.Cliente.Email, subject, body, ct);
    }

    private string GenerarHtmlPedidoRecibido(Cliente cliente, Venta venta)
    {
        var html = new StringBuilder();

        DateTime fechaVenta = ConvertirFechaAGuatemala(venta.Fecha);
        
        html.Append($@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Pedido Recibido - SuperBodega</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f8f9fa; }}
        .container {{ max-width: 650px; margin: 20px auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 25px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); color: white; padding: 40px 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 32px; font-weight: bold; }}
        .header p {{ margin: 10px 0 0 0; font-size: 18px; opacity: 0.95; }}
        .content {{ padding: 40px 30px; }}
        .order-info {{ background: linear-gradient(145deg, #f8f9fa, #e9ecef); border-radius: 10px; padding: 25px; margin-bottom: 25px; border-left: 5px solid #4CAF50; }}
        .order-number {{ font-size: 28px; font-weight: bold; color: #4CAF50; margin-bottom: 15px; }}
        .customer-info {{ margin-bottom: 25px; background-color: #fff; border-radius: 8px; padding: 20px; border: 1px solid #e9ecef; }}
        .customer-info h3 {{ color: #333; margin-bottom: 15px; border-bottom: 2px solid #4CAF50; padding-bottom: 8px; display: flex; align-items: center; }}
        .items-table {{ width: 100%; border-collapse: collapse; margin: 25px 0; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        .items-table th {{ background: linear-gradient(135deg, #4CAF50, #45a049); color: white; padding: 15px 12px; text-align: left; font-weight: 600; }}
        .items-table td {{ padding: 15px 12px; border-bottom: 1px solid #e9ecef; }}
        .items-table tr:nth-child(even) {{ background-color: #f8f9fa; }}
        .items-table tr:hover {{ background-color: #e8f5e8; }}
        .total-section {{ background: linear-gradient(145deg, #e8f5e8, #d4edda); border-radius: 10px; padding: 25px; margin: 25px 0; border: 2px solid #4CAF50; }}
        .total-amount {{ font-size: 24px; font-weight: bold; color: #2e7d32; }}
        .status-badge {{ display: inline-block; background: linear-gradient(135deg, #ffc107, #ffb300); color: #212529; padding: 10px 20px; border-radius: 25px; font-weight: bold; font-size: 14px; }}
        .footer {{ background-color: #2c3e50; color: white; padding: 30px; text-align: center; }}
        .footer p {{ margin: 8px 0; }}
        .contact-info {{ margin-top: 20px; }}
        .contact-info a {{ color: #4CAF50; text-decoration: none; }}
        .icon {{ font-size: 20px; margin-right: 10px; }}
        .highlight-box {{ background-color: #d1ecf1; border: 1px solid #bee5eb; border-radius: 10px; padding: 20px; margin: 25px 0; }}
        .highlight-box h4 {{ color: #0c5460; margin-top: 0; }}
        .highlight-box ul {{ color: #0c5460; margin-bottom: 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🛒 SuperBodega</h1>
            <p>¡Gracias por tu confianza!</p>
        </div>
        
        <div class='content'>
            <div class='order-info'>
                <div class='order-number'>Pedido #{venta.Id}</div>
                <p><strong>📅 Fecha:</strong> {fechaVenta:dd/MM/yyyy hh:mm tt}</p>
                <p><strong>📊 Estado:</strong> <span class='status-badge'>✅ Recibido</span></p>
            </div>

            <div class='customer-info'>
                <h3><span class='icon'>👤</span> Información del Cliente</h3>
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 15px;'>
                    <div>
                        <p><strong>Nombre:</strong> {cliente.Nombre} {cliente.Apellido}</p>
                        <p><strong>Email:</strong> {cliente.Email}</p>
                    </div>
                    <div>
                        <p><strong>Teléfono:</strong> {cliente.Telefono}</p>
                        <p><strong>Dirección:</strong> {cliente.Direccion}</p>
                    </div>
                </div>
            </div>

            <h3><span class='icon'>📦</span> Productos Pedidos</h3>
            <table class='items-table'>
                <thead>
                    <tr>
                        <th>Producto</th>
                        <th>Cantidad</th>
                        <th>Precio Unit.</th>
                        <th>Subtotal</th>
                    </tr>
                </thead>
                <tbody>");

        foreach (var item in venta.Productos)
        {
            html.Append($@"
                    <tr>
                        <td><strong>{item.Producto.Nombre}</strong></td>
                        <td>{item.Cantidad}</td>
                        <td>Q{item.PrecioUnitario:F2}</td>
                        <td><strong>Q{item.Total:F2}</strong></td>
                    </tr>");
        }

        html.Append($@"
                </tbody>
            </table>

            <div class='total-section'>
                <div style='text-align: center;'>
                    <p style='font-size: 18px; margin-bottom: 10px;'>Subtotal: Q{venta.Total:F2}</p>
                    <p class='total-amount'>Total: Q{venta.Total:F2}</p>
                </div>
            </div>

            <div class='highlight-box'>
                <h4>🚀 Próximos pasos:</h4>
                <ul>
                    <li>✨ Estamos preparando tu pedido con mucho cuidado</li>
                    <li>📧 Te notificaremos cuando esté listo para envío</li>
                    <li>🚚 Recibirás actualizaciones del estado por email</li>
                    <li>📞 Puedes contactarnos si tienes preguntas</li>
                </ul>
            </div>

            <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 15px; margin: 20px 0; text-align: center;'>
                <p style='margin: 0; color: #856404;'><strong>💡 Tip:</strong> Guarda este correo como comprobante de tu pedido</p>
            </div>
        </div>

        <div class='footer'>
            <p><strong>SuperBodega</strong></p>
            <p>Tu tienda de confianza desde siempre</p>
            <div class='contact-info'>
                <p>📞 Teléfono: +502 1234-5678</p>
                <p>📧 Email: <a href='mailto:info@superbodega.com'>info@superbodega.com</a></p>
                <p>🌐 Sitio web: <a href='#'>www.superbodega.com</a></p>
            </div>
        </div>
    </div>
</body>
</html>");

        return html.ToString();
    }

    private string GenerarHtmlActualizacionEstado(Venta venta, string nuevoEstado)
    {
        var (estadoTexto, estadoColor, estadoIcon, mensaje) = ObtenerInfoEstado(nuevoEstado);
        
        var html = new StringBuilder();

        DateTime fechaVenta = ConvertirFechaAGuatemala(venta.Fecha);
        
        html.Append($@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Actualización de Pedido - SuperBodega</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f8f9fa; }}
        .container {{ max-width: 650px; margin: 20px auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 25px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, {estadoColor} 0%, #2c3e50 100%); color: white; padding: 40px 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 32px; font-weight: bold; }}
        .header p {{ margin: 10px 0 0 0; font-size: 18px; opacity: 0.95; }}
        .content {{ padding: 40px 30px; }}
        .status-update {{ background: linear-gradient(145deg, #fff, #f8f9fa); border-left: 6px solid {estadoColor}; padding: 30px; margin: 25px 0; border-radius: 0 12px 12px 0; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .status-icon {{ font-size: 64px; text-align: center; margin-bottom: 20px; }}
        .order-info {{ background: linear-gradient(145deg, #e9ecef, #f8f9fa); border-radius: 10px; padding: 25px; margin: 25px 0; border: 1px solid #dee2e6; }}
        .items-summary {{ background-color: #f8f9fa; border-radius: 10px; padding: 20px; margin: 20px 0; border: 1px solid #e9ecef; }}
        .timeline {{ background-color: #fff; border-radius: 10px; padding: 25px; margin: 25px 0; border: 1px solid #e9ecef; }}
        .footer {{ background-color: #2c3e50; color: white; padding: 30px; text-align: center; }}
        .contact-info a {{ color: #4CAF50; text-decoration: none; }}
        .highlight-box {{ background-color: #d1ecf1; border: 1px solid #bee5eb; border-radius: 10px; padding: 20px; margin: 25px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📦 SuperBodega</h1>
            <p>Actualización de tu pedido</p>
        </div>
        
        <div class='content'>
            <div class='status-update'>
                <div class='status-icon'>{estadoIcon}</div>
                <h2 style='text-align: center; color: {estadoColor}; margin: 0; font-size: 28px;'>{estadoTexto}</h2>
                <p style='text-align: center; font-size: 18px; margin: 15px 0 0 0; color: #666;'>{mensaje}</p>
            </div>

            <div class='order-info'>
                <h3>📋 Detalles del Pedido</h3>
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 15px;'>
                    <div>
                        <p><strong>Número de Pedido:</strong> #{venta.Id}</p>
                        <p><strong>Cliente:</strong> {venta.Cliente.Nombre} {venta.Cliente.Apellido}</p>
                    </div>
                    <div>
                        <p><strong>Fecha del Pedido:</strong> {fechaVenta:dd/MM/yyyy hh:mm tt}</p>
                        <p><strong>Total:</strong> <span style='color: #4CAF50; font-weight: bold; font-size: 18px;'>Q{venta.Total:F2}</span></p>
                    </div>
                </div>
            </div>

            <div class='items-summary'>
                <h4>🛍️ Resumen de Productos ({venta.Productos.Count} artículos)</h4>");

        foreach (var item in venta.Productos.Take(5))
        {
            html.Append($@"
                <p style='padding: 8px 0; border-bottom: 1px solid #e9ecef;'>
                    • <strong>{item.Producto.Nombre}</strong> (x{item.Cantidad}) - Q{item.Total:F2}
                </p>");
        }

        if (venta.Productos.Count > 5)
        {
            html.Append($@"
                <p style='font-style: italic; color: #666; margin-top: 10px;'>... y {venta.Productos.Count - 5} productos más</p>");
        }

        var proximoPaso = ObtenerProximoPaso(nuevoEstado);
        var estadoTimeline = GenerarTimeline(nuevoEstado);
        
        html.Append($@"
            </div>

            <div class='timeline'>
                <h4>📊 Estado del Pedido</h4>
                {estadoTimeline}
            </div>

            <div class='highlight-box'>
                <h4 style='color: #0c5460; margin-top: 0;'>🔄 Próximo paso:</h4>
                <p style='color: #0c5460; margin-bottom: 0; font-size: 16px;'>{proximoPaso}</p>
            </div>

            <div style='text-align: center; margin: 30px 0; padding: 25px; background-color: #f8f9fa; border-radius: 8px;'>
                <h4 style='color: #333; margin-bottom: 15px;'>¿Tienes alguna pregunta sobre tu pedido?</h4>
                <p style='color: #666; margin-bottom: 15px;'>Nuestro equipo está aquí para ayudarte</p>
                <p style='margin: 0;'>
                    <a href='mailto:info@superbodega.com' style='background-color: #4CAF50; color: white; padding: 12px 25px; text-decoration: none; border-radius: 25px; font-weight: bold;'>
                        📧 Contactar Soporte
                    </a>
                </p>
            </div>
        </div>

        <div class='footer'>
            <p><strong>SuperBodega</strong></p>
            <p>Tu tienda de confianza desde siempre</p>
            <div class='contact-info'>
                <p>📞 Teléfono: +502 1234-5678</p>
                <p>📧 Email: <a href='mailto:info@superbodega.com'>info@superbodega.com</a></p>
                <p>🌐 Sitio web: <a href='#'>www.superbodega.com</a></p>
            </div>
        </div>
    </div>
</body>
</html>");

        return html.ToString();
    }

    private (string texto, string color, string icon, string mensaje) ObtenerInfoEstado(string estado)
    {
        return estado.ToLower() switch
        {
            "despachado" => ("¡Pedido Despachado!", "#FF9800", "🚚", "Tu pedido salió de nuestra bodega y va en camino hacia ti."),
            "entregado" => ("¡Pedido Entregado!", "#4CAF50", "✅", "Tu pedido ha sido entregado exitosamente. ¡Gracias por confiar en nosotros!"),
            "cancelado" => ("Pedido Cancelado", "#F44336", "❌", "Tu pedido ha sido cancelado. Si tienes preguntas, contáctanos."),
            _ => ("Estado Actualizado", "#2196F3", "📋", "El estado de tu pedido ha sido actualizado.")
        };
    }

    private string ObtenerProximoPaso(string estado)
    {
        return estado.ToLower() switch
        {
            "despachado" => "🚛 Tu pedido está en camino. Te notificaremos cuando sea entregado.",
            "entregado" => "🎉 ¡Tu pedido está completo! Esperamos que disfrutes tus productos.",
            "cancelado" => "🛒 Si necesitas hacer un nuevo pedido, visita nuestra tienda.",
            _ => "📧 Te mantendremos informado sobre cualquier cambio en tu pedido."
        };
    }

    private string GenerarTimeline(string estado)
    {
        var recibidoClass = "style='color: #4CAF50; font-weight: bold;'";
        var despachadoClass = estado.ToLower() == "despachado" || estado.ToLower() == "entregado" ? "style='color: #4CAF50; font-weight: bold;'" : "style='color: #ccc;'";
        var entregadoClass = estado.ToLower() == "entregado" ? "style='color: #4CAF50; font-weight: bold;'" : "style='color: #ccc;'";

        return $@"
            <div style='display: flex; justify-content: space-between; align-items: center; margin: 20px 0; padding: 0 20px;'>
                <div style='text-align: center; flex: 1; margin: 0;'>
                    <div style='width: 40px; height: 40px; border-radius: 50%; background-color: #4CAF50; margin: 0 auto 15px; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 16px; line-height: 40px;'>✓</div>
                    <p {recibidoClass}>Recibido</p>
                </div>
                <div style='flex: 2; height: 3px; background-color: {(estado.ToLower() != "recibido" ? "#4CAF50" : "#ccc")}; margin: 0 10px;'></div>
                <div style='text-align: center; flex: 1; margin: 0;'>
                    <div style='width: 40px; height: 40px; border-radius: 50%; background-color: {(estado.ToLower() == "despachado" || estado.ToLower() == "entregado" ? "#4CAF50" : "#ccc")}; margin: 0 auto 15px; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 16px; line-height: 40px;'>{(estado.ToLower() == "despachado" || estado.ToLower() == "entregado" ? "✓" : "")}</div>
                    <p {despachadoClass}>Despachado</p>
                </div>
                <div style='flex: 2; height: 3px; background-color: {(estado.ToLower() == "entregado" ? "#4CAF50" : "#ccc")}; margin: 0 10px;'></div>
                <div style='text-align: center; flex: 1; margin: 0;'>
                    <div style='width: 40px; height: 40px; border-radius: 50%; background-color: {(estado.ToLower() == "entregado" ? "#4CAF50" : "#ccc")}; margin: 0 auto 15px; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 16px; line-height: 40px;'>{(estado.ToLower() == "entregado" ? "✓" : "")}</div>
                    <p {entregadoClass}>Entregado</p>
                </div>
            </div>";
    }
}