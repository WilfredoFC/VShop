//using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using VShop.Application.Dtos.Email;
using VShop.Application.Dtos.Pedido;
using VShop.Application.Interfaces;
using VShop.Domain.Setting;

namespace VShop.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        //private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
            //_logger = logger;
        }

        public async Task SendAsync(EmailRequestDto emailRequestDto)
        {
            try
            {
                emailRequestDto.ToRange ??= new List<string>();

                emailRequestDto.ToRange?.Add(emailRequestDto.To ?? "");

                MimeMessage email = new()
                {
                    Sender = MailboxAddress.Parse(_mailSettings.EmailFrom),
                    Subject = emailRequestDto.Subject
                };

                foreach (var toItem in emailRequestDto.ToRange ?? [])
                {
                    email.To.Add(MailboxAddress.Parse(toItem));
                }

                BodyBuilder builder = new()
                {
                    HtmlBody = emailRequestDto.HtmlBody
                };
                email.Body = builder.ToMessageBody();

                using MailKit.Net.Smtp.SmtpClient smtpClient = new();
                await smtpClient.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"A ocurrido una excepcion {ex.Message}.");
            }
        }

        public async Task SendReceiboAsync(PedidoDto pedido, string emailCliente, string nombreCliente)
        {
            try
            {
                var htmlBody = GenerateReceiboHtml(pedido, nombreCliente);
                
                var emailRequest = new EmailRequestDto
                {
                    To = emailCliente,
                    Subject = $"Recibo de Compra - Pedido #{pedido.NumeroPedido}",
                    HtmlBody = htmlBody
                };

                await SendAsync(emailRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando recibo: {ex.Message}");
            }
        }

        public async Task SendNotificacionNuevoPedidoAsync(PedidoDto pedido, string emailAdmin, string nombreCliente)
        {
            try
            {
                var htmlBody = GenerateNotificacionAdminHtml(pedido, nombreCliente);
                
                var emailRequest = new EmailRequestDto
                {
                    To = emailAdmin,
                    Subject = $"Nuevo Pedido - #{pedido.NumeroPedido}",
                    HtmlBody = htmlBody
                };

                await SendAsync(emailRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando notificación al admin: {ex.Message}");
            }
        }

        private string GenerateReceiboHtml(PedidoDto pedido, string nombreCliente)
        {
            var detallesHtml = "";
            if (pedido.Detalles != null)
            {
                foreach (var detalle in pedido.Detalles)
                {
                    detallesHtml += $@"
                        <tr>
                            <td>{detalle.Producto?.Nombre ?? "Producto"}</td>
                            <td>{detalle.Cantidad}</td>
                            <td>RD${detalle.PrecioUnitario:F2}</td>
                            <td>RD${detalle.Subtotal:F2}</td>
                        </tr>";
                }
            }

            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                    table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                    th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
                    th {{ background-color: #f2f2f2; }}
                    .summary {{ float: right; width: 200px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Recibo de Compra</h1>
                    </div>
                    
                    <h2>Hola {nombreCliente},</h2>
                    <p>Gracias por tu compra. Aquí está el detalle de tu pedido:</p>
                    
                    <p><strong>Número de Pedido:</strong> {pedido.NumeroPedido}</p>
                    <p><strong>Fecha:</strong> {pedido.FechaPedido:dd/MM/yyyy HH:mm}</p>
                    
                    <h3>Productos:</h3>
                    <table>
                        <thead>
                            <tr>
                                <th>Producto</th>
                                <th>Cantidad</th>
                                <th>Precio Unitario</th>
                                <th>Subtotal</th>
                            </tr>
                        </thead>
                        <tbody>
                            {detallesHtml}
                        </tbody>
                    </table>
                    
                    <div class='summary'>
                        <p><strong>Subtotal:</strong> RD${pedido.Subtotal:F2}</p>
                        <p><strong>Impuestos (ITBIS):</strong> RD${pedido.Impuestos:F2}</p>
                        <p><strong>Total:</strong> RD${pedido.Total:F2}</p>
                    </div>
                    
                    <div style='clear: both; margin-top: 20px;'>
                        <h3>Dirección de Envío:</h3>
                        <p>{pedido.DireccionEnvio}<br/>{pedido.Ciudad}</p>
                        <p><strong>Teléfono:</strong> {pedido.TelefonoContacto}</p>
                    </div>
                    
                    <p style='margin-top: 40px; color: #666;'>
                        Si tienes preguntas sobre tu pedido, contactanos.
                    </p>
                </div>
            </body>
            </html>";
        }

        private string GenerateNotificacionAdminHtml(PedidoDto pedido, string nombreCliente)
        {
            var detallesHtml = "";
            if (pedido.Detalles != null)
            {
                foreach (var detalle in pedido.Detalles)
                {
                    detallesHtml += $@"
                        <tr>
                            <td>{detalle.Producto?.Nombre ?? "Producto"}</td>
                            <td>{detalle.Cantidad}</td>
                            <td>RD${detalle.PrecioUnitario:F2}</td>
                            <td>RD${detalle.Subtotal:F2}</td>
                        </tr>";
                }
            }

            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ max-width: 700px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                    table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                    th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
                    th {{ background-color: #f2f2f2; }}
                    .alert {{ background-color: #fff3cd; padding: 10px; margin: 10px 0; border-radius: 4px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>⚠️ Nuevo Pedido Recibido</h1>
                    </div>
                    
                    <div class='alert'>
                        <strong>Acción Requerida:</strong> Hay un nuevo pedido pendiente de procesamiento.
                    </div>
                    
                    <h3>Información del Cliente:</h3>
                    <p><strong>Nombre:</strong> {nombreCliente}</p>
                    
                    <h3>Detalles del Pedido:</h3>
                    <p><strong>Número:</strong> {pedido.NumeroPedido}</p>
                    <p><strong>Fecha:</strong> {pedido.FechaPedido:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Estado:</strong> {pedido.Estado}</p>
                    <p><strong>Método de Pago:</strong> {pedido.MetodoPago}</p>
                    
                    <h3>Productos:</h3>
                    <table>
                        <thead>
                            <tr>
                                <th>Producto</th>
                                <th>Cantidad</th>
                                <th>Precio</th>
                                <th>Subtotal</th>
                            </tr>
                        </thead>
                        <tbody>
                            {detallesHtml}
                        </tbody>
                    </table>
                    
                    <h3>Resumen Financiero:</h3>
                    <p><strong>Subtotal:</strong> RD${pedido.Subtotal:F2}</p>
                    <p><strong>Impuestos:</strong> RD${pedido.Impuestos:F2}</p>
                    <p><strong>Total:</strong> RD${pedido.Total:F2}</p>
                    
                    <h3>Dirección de Envío:</h3>
                    <p>{pedido.DireccionEnvio}<br/>{pedido.Ciudad}</p>
                    <p><strong>Teléfono:</strong> {pedido.TelefonoContacto}</p>
                    
                    <p style='margin-top: 40px; color: #666;'>
                        Accede al panel administrativo para ver más detalles y cambiar el estado del pedido.
                    </p>
                </div>
            </body>
            </html>";
        }
    }
}
