using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Interfaces;

namespace VShop.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IPedidoService pedidoService, ILogger<AdminController> logger)
        {
            _pedidoService = pedidoService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Pedidos");
        }

        // GET: /Admin/Pedidos
        public async Task<IActionResult> Pedidos()
        {
            try
            {
                var pedidos = await _pedidoService.GetAllListDto();
                // Ordenar por fecha descendente
                var pedidosOrdenados = pedidos.OrderByDescending(p => p.FechaPedido).ToList();
                return View(pedidosOrdenados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo lista de pedidos");
                return BadRequest("Error al obtener los pedidos");
            }
        }

        // GET: /Admin/DetallesPedido/{id}
        public async Task<IActionResult> DetallesPedido(int id)
        {
            try
            {
                var pedido = await _pedidoService.GetPedidoWithDetailsAsync(id);
                if (pedido == null)
                    return NotFound();

                return View(pedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalles del pedido");
                return BadRequest("Error al obtener los detalles del pedido");
            }
        }

        // POST: /Admin/ActualizarEstadoPedido
        [HttpPost]
        public async Task<IActionResult> ActualizarEstadoPedido(int pedidoId, string nuevoEstado)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nuevoEstado))
                    return BadRequest(new { success = false, message = "Estado inválido" });

                var estadosValidos = new[] { "Pendiente", "Confirmado", "EnProceso", "Enviado", "Entregado", "Cancelado", "ErrorPago", "Rechazado" };
                if (!estadosValidos.Contains(nuevoEstado))
                    return BadRequest(new { success = false, message = "Estado no reconocido" });

                var resultado = await _pedidoService.UpdateEstadoAsync(pedidoId, nuevoEstado);
                if (resultado == null)
                    return NotFound(new { success = false, message = "Pedido no encontrado" });

                _logger.LogInformation("Estado del pedido {0} actualizado a {1}", pedidoId, nuevoEstado);

                return Ok(new { success = true, message = "Estado actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado del pedido");
                return BadRequest(new { success = false, message = "Error al actualizar el estado" });
            }
        }

        // GET: /Admin/BuscarPedidos
        [HttpGet]
        public async Task<IActionResult> BuscarPedidos(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                    return BadRequest(new { success = false, message = "Término de búsqueda vacío" });

                var allPedidos = await _pedidoService.GetAllListDto();
                var resultados = allPedidos
                    .Where(p => p.NumeroPedido.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                                p.UsuarioId.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                                p.Estado.Contains(termino, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(p => p.FechaPedido)
                    .ToList();

                return Ok(new { success = true, pedidos = resultados });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando pedidos");
                return BadRequest(new { success = false, message = "Error al buscar pedidos" });
            }
        }

        // GET: /Admin/FiltrarPorEstado
        [HttpGet]
        public async Task<IActionResult> FiltrarPorEstado(string estado)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(estado))
                    return BadRequest(new { success = false, message = "Estado vacío" });

                var allPedidos = await _pedidoService.GetAllListDto();
                var resultados = allPedidos
                    .Where(p => p.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(p => p.FechaPedido)
                    .ToList();

                return Ok(new { success = true, pedidos = resultados, total = resultados.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtrando pedidos por estado");
                return BadRequest(new { success = false, message = "Error al filtrar pedidos" });
            }
        }
    }
}
