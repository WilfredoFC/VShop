// ========== DESHABILITADO: Controlador de Pagos Azul (Pagos desactivados) ==========
// Este controlador fue comentado porque las credenciales de Azul aún no están disponibles
// Descomentar cuando las credenciales estén disponibles

/*
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VShop.Application.Dtos.PedidoDetalle;
using VShop.Application.Interfaces;
using VShop.Identity.Entities;
using VShop.Models;

namespace VShop.Controllers
{
    [Authorize]
    public class PagoController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly ICarritoItemService _carritoService;
        private readonly IPedidoDetalleService _pedidoDetalleService;
        private readonly IAzulService _azulService;
        private readonly IEmailService _emailService;
        private readonly ILogger<PagoController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public PagoController(
            IPedidoService pedidoService,
            ICarritoItemService carritoService,
            IPedidoDetalleService pedidoDetalleService,
            IAzulService azulService,
            IEmailService emailService,
            ILogger<PagoController> logger,
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            IMapper mapper)
        {
            _pedidoService = pedidoService;
            _carritoService = carritoService;
            _pedidoDetalleService = pedidoDetalleService;
            _azulService = azulService;
            _emailService = emailService;
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Iniciar([FromBody] IniciarPagoRequestViewModel request)
        {
            try
            {
                // Obtener usuario actual
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { success = false, message = "Usuario no autenticado" });

                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario == null)
                    return Unauthorized(new { success = false, message = "Usuario no encontrado" });

                // Validar campos requeridos del envío
                if (string.IsNullOrWhiteSpace(request.Direccion) || 
                    string.IsNullOrWhiteSpace(request.Ciudad) || 
                    string.IsNullOrWhiteSpace(request.Telefono))
                {
                    return BadRequest(new { success = false, message = "Dirección, ciudad y teléfono son requeridos" });
                }

                // Obtener items del carrito
                var carritoItems = await _carritoService.GetDtoByUserId(usuarioId);
                if (carritoItems == null || !carritoItems.Any())
                    return BadRequest(new { success = false, message = "El carrito está vacío" });

                // Calcular totales
                decimal subtotal = carritoItems.Sum(i => i.Producto.Precio * i.Cantidad);
                decimal tasaImpuesto = decimal.TryParse(_configuration["Payment:TaxRate"] ?? "0.18", out var tasa) ? tasa : 0.18m;
                decimal impuestos = subtotal * tasaImpuesto;
                decimal total = subtotal + impuestos;

                // Crear número de pedido único
                var prefijo = _configuration["Payment:OrderPrefix"] ?? "PED-";
                var numeroPedido = prefijo + DateTime.Now.Ticks.ToString();

                // Crear DTO de Pedido
                var pedidoDto = new VShop.Application.Dtos.Pedido.PedidoDto
                {
                    NumeroPedido = numeroPedido,
                    UsuarioId = usuarioId,
                    FechaPedido = DateTime.UtcNow,
                    Estado = "Pendiente",
                    Subtotal = subtotal,
                    Impuestos = impuestos,
                    Total = total,
                    MetodoPago = "Azul",
                    DireccionEnvio = request.Direccion,
                    Ciudad = request.Ciudad,
                    TelefonoContacto = request.Telefono,
                    Notas = request.Notas ?? "",
                    Detalles = carritoItems.Select(i => new PedidoDetalleDto
                    {
                        ProductoId = i.ProductoId,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.Producto.Precio,
                        Subtotal = i.Producto.Precio * i.Cantidad
                    }).ToList()
                };

                // Guardar pedido en BD
                var savedPedidoDto = await _pedidoService.SaveDtoAsync(pedidoDto);

                // Guardar detalles del pedido
                if (pedidoDto.Detalles != null && pedidoDto.Detalles.Any())
                {
                    foreach (var detalle in pedidoDto.Detalles)
                    {
                        detalle.PedidoId = savedPedidoDto.Id;
                        await _pedidoDetalleService.SaveDtoAsync(detalle);
                    }
                }

                // Solicitar token a Azul
                var azulResponse = await _azulService.SolicitarTokenAsync(savedPedidoDto);
                if (!azulResponse.Success)
                {
                    _logger.LogError("Error de Azul: {0}", azulResponse.ErrorMessage);
                    // Actualizar estado del pedido a error
                    await _pedidoService.UpdateEstadoAsync(savedPedidoDto.Id, "ErrorPago");
                    return BadRequest(new { success = false, message = azulResponse.ErrorMessage });
                }

                // Guardar token de Azul en el pedido
                savedPedidoDto.AzulToken = azulResponse.Token;
                await _pedidoService.UpdateDtoAsync(savedPedidoDto, savedPedidoDto.Id);

                _logger.LogInformation("Pedido iniciado: {0} para usuario {1}", numeroPedido, usuarioId);

                // Retornar la URL para redirigir a Azul
                return Ok(new { success = true, pagoUrl = azulResponse.Url, ordenId = savedPedidoDto.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Iniciar pago");
                return BadRequest(new { success = false, message = "Error procesando el pago: " + ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [IgnoreAntiforgeryToken] // Importante para webhooks de Azul
        public async Task<IActionResult> Notificacion()
        {
            try
            {
                // Leer formulario enviado por Azul
                var form = await Request.ReadFormAsync();

                // Procesar la notificación
                var result = _azulService.ProcesarNotificacion(form);

                // Buscar pedido por número
                var pedido = await _pedidoService.GetPedidoByNumeroAsync(result.OrderNumber);
                if (pedido == null)
                {
                    _logger.LogWarning("Pedido no encontrado para notificación: {0}", result.OrderNumber);
                    return Ok(); // Responder OK igual para que Azul no reintente
                }

                // Actualizar campos de Azul en el pedido
                pedido.AzulAuthorizationCode = result.AuthorizationCode;
                pedido.AzulResponseCode = result.ResponseCode;
                pedido.AzulIsoCode = result.IsoCode;
                pedido.AzulErrorDescripcion = result.ErrorDescription;

                if (result.IsoCode == "00") // Pago exitoso
                {
                    pedido.Estado = "Confirmado";
                    pedido.FechaPago = DateTime.UtcNow;

                    // Obtener usuario para enviar correos
                    var usuario = await _userManager.FindByIdAsync(pedido.UsuarioId);
                    if (usuario != null)
                    {
                        // Enviar recibo al cliente
                        try
                        {
                            await _emailService.SendReceiboAsync(pedido, usuario.Email, usuario.FirstName + " " + usuario.LastName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error enviando recibo a cliente");
                        }

                        // Enviar notificación al admin
                        try
                        {
                            var emailAdmin = _configuration["Admin:Email"] ?? "admin@example.com";
                            await _emailService.SendNotificacionNuevoPedidoAsync(pedido, emailAdmin, usuario.FirstName + " " + usuario.LastName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error enviando notificación al admin");
                        }
                    }

                    _logger.LogInformation("Pago confirmado para pedido: {0}", result.OrderNumber);
                }
                else
                {
                    pedido.Estado = "Rechazado";
                    _logger.LogWarning("Pago rechazado para pedido: {0}. Código ISO: {1}", result.OrderNumber, result.IsoCode);
                }

                // Actualizar pedido en BD
                await _pedidoService.UpdateDtoAsync(pedido, pedido.Id);

                // Responder OK para que Azul sepa que recibimos
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando notificación de Azul");
                return Ok(); // Responder OK igual para no generar reintento infinito
            }
        }

        [HttpGet]
        public async Task<IActionResult> Resultado(string orderNumber)
        {
            try
            {
                // Buscar pedido
                var pedido = await _pedidoService.GetPedidoByNumeroAsync(orderNumber);
                if (pedido == null)
                {
                    return NotFound(new { message = "Pedido no encontrado" });
                }

                // Retornar vista con resultado del pago
                return View(pedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Resultado de pago");
                return BadRequest(new { message = "Error procesando resultado del pago" });
            }
        }
    }
}
*/
// ==================================================================================