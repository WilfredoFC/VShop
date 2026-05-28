using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VShop.Application.Interfaces;
using VShop.Application.Dtos.CarritoItem;
using VShop.Application.Dtos.Producto;

namespace VShop.Controllers
{
    [Authorize]
    public class CarritoController : Controller
    {
        private readonly ICarritoItemService _carritoService;
        private readonly IProductoService _productoService;
        private readonly ILogger<CarritoController> _logger;

        public CarritoController(
            ICarritoItemService carritoService,
            IProductoService productoService,
            ILogger<CarritoController> logger)
        {
            _carritoService = carritoService;
            _productoService = productoService;
            _logger = logger;
        }

        // GET: /Carrito/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioId))
                    return Redirect("/Login");

                var carrito = await _carritoService.GetDtoByUserId(usuarioId);
                return View(carrito ?? new List<CarritoItemDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo carrito");
                return BadRequest("Error al obtener el carrito");
            }
        }

        // POST: /Carrito/AgregarProducto
        [HttpPost]
        public async Task<IActionResult> AgregarProducto([FromBody] AgregarProductoRequest request)
        {
            try
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { success = false, message = "Usuario no autenticado" });

                if (request.ProductoId <= 0 || request.Cantidad <= 0)
                    return BadRequest(new { success = false, message = "Producto o cantidad inválida" });

                // Validar que el producto existe
                var producto = await _productoService.GetDtoById(request.ProductoId);
                if (producto == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                // Validar stock
                if (producto.Stock < request.Cantidad)
                    return BadRequest(new { success = false, message = $"Stock insuficiente. Disponible: {producto.Stock}" });

                // Crear o actualizar item del carrito
                var carritoItem = new CarritoItemDto
                {
                    ProductoId = request.ProductoId,
                    UsuarioId = usuarioId,
                    Cantidad = request.Cantidad,
                    FechaAgregado = DateTime.UtcNow
                };

                await _carritoService.SaveDtoAsync(carritoItem);

                _logger.LogInformation("Producto {0} agregado al carrito del usuario {1}", request.ProductoId, usuarioId);

                return Ok(new { success = true, message = "Producto agregado al carrito" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando producto al carrito");
                return BadRequest(new { success = false, message = "Error al agregar el producto" });
            }
        }

        // POST: /Carrito/ActualizarCantidad
        [HttpPost]
        public async Task<IActionResult> ActualizarCantidad([FromBody] ActualizarCantidadRequest request)
        {
            try
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { success = false, message = "Usuario no autenticado" });

                if (request.CarritoItemId <= 0 || request.NuevaCantidad <= 0)
                    return BadRequest(new { success = false, message = "Datos inválidos" });

                var item = await _carritoService.GetDtoById(request.CarritoItemId);
                if (item == null || item.UsuarioId != usuarioId)
                    return NotFound(new { success = false, message = "Item del carrito no encontrado" });

                // Validar stock
                var producto = await _productoService.GetDtoById(item.ProductoId);
                if (producto.Stock < request.NuevaCantidad)
                    return BadRequest(new { success = false, message = $"Stock insuficiente. Disponible: {producto.Stock}" });

                item.Cantidad = request.NuevaCantidad;
                await _carritoService.UpdateDtoAsync(item, request.CarritoItemId);

                _logger.LogInformation("Cantidad actualizada en carrito del usuario {0}", usuarioId);

                return Ok(new { success = true, message = "Cantidad actualizada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando cantidad");
                return BadRequest(new { success = false, message = "Error al actualizar la cantidad" });
            }
        }

        // POST: /Carrito/EliminarProducto
        [HttpPost]
        public async Task<IActionResult> EliminarProducto([FromBody] EliminarProductoRequest request)
        {
            try
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { success = false, message = "Usuario no autenticado" });

                var item = await _carritoService.GetDtoById(request.CarritoItemId);
                if (item == null || item.UsuarioId != usuarioId)
                    return NotFound(new { success = false, message = "Item del carrito no encontrado" });

                await _carritoService.DeleteHardDtoAsync(request.CarritoItemId);

                _logger.LogInformation("Producto eliminado del carrito del usuario {0}", usuarioId);

                return Ok(new { success = true, message = "Producto eliminado del carrito" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando producto");
                return BadRequest(new { success = false, message = "Error al eliminar el producto" });
            }
        }

        // POST: /Carrito/VaciarCarrito
        [HttpPost]
        public async Task<IActionResult> VaciarCarrito()
        {
            try
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { success = false, message = "Usuario no autenticado" });

                var carrito = await _carritoService.GetDtoByUserId(usuarioId);
                foreach (var item in carrito ?? new List<CarritoItemDto>())
                {
                    await _carritoService.DeleteHardDtoAsync(item.Id);
                }

                _logger.LogInformation("Carrito vaciado para usuario {0}", usuarioId);

                return Ok(new { success = true, message = "Carrito vaciado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error vaciando carrito");
                return BadRequest(new { success = false, message = "Error al vaciar el carrito" });
            }
        }

        // GET: /Carrito/ObtenerCarrito
        [HttpGet]
        public async Task<IActionResult> ObtenerCarrito()
        {
            try
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { success = false, message = "Usuario no autenticado" });

                var carrito = await _carritoService.GetDtoByUserId(usuarioId);
                var totalItemscarrito = carrito?.Count ?? 0;
                var subtotal = carrito?.Sum(i => i.Producto.Precio * i.Cantidad) ?? 0;
                var impuestos = subtotal * 0.18m;
                var total = subtotal + impuestos;

                return Ok(new
                {
                    success = true,
                    items = carrito,
                    totalItems = totalItemscarrito,
                    subtotal = subtotal,
                    impuestos = impuestos,
                    total = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo carrito");
                return BadRequest(new { success = false, message = "Error al obtener el carrito" });
            }
        }
    }
}
