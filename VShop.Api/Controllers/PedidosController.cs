using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Dtos.Pedido;
using VShop.Application.Dtos.PedidoDetalle;
using VShop.Application.Interfaces;

namespace VShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController(
    IPedidoService pedidoService,
    IPedidoDetalleService pedidoDetalleService,
    ICarritoItemService carritoService,
    IProductoService productoService,
    IConfiguration config) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET api/pedidos — pedidos del usuario autenticado
    [HttpGet]
    public async Task<IActionResult> GetMisPedidos()
    {
        var pedidos = await pedidoService.GetPedidosByUsuarioAsync(UserId);
        return Ok(pedidos.Select(p => new
        {
            p.Id, p.NumeroPedido, p.Estado, p.FechaPedido,
            p.Subtotal, p.Impuestos, p.Total
        }));
    }

    // GET api/pedidos/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pedido = await pedidoService.GetPedidoWithDetailsAsync(id);
        if (pedido == null || pedido.UsuarioId != UserId) return NotFound();
        return Ok(pedido);
    }

    // POST api/pedidos — crear pedido desde carrito
    [HttpPost]
    public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoRequest req)
    {
        var carritoItems = await carritoService.GetDtoByUserId(UserId);
        if (carritoItems.Count == 0)
            return BadRequest(new { error = "El carrito está vacío." });

        var taxRate = decimal.Parse(config["Payment:TaxRate"] ?? "0.18");
        var prefix = config["Payment:OrderPrefix"] ?? "PED-";
        var numeroPedido = $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        decimal subtotal = 0;
        foreach (var item in carritoItems)
        {
            var producto = await productoService.GetDtoById(item.ProductoId);
            subtotal += (producto?.PrecioFinal ?? 0) * item.Cantidad;
        }

        var impuestos = subtotal * taxRate;

        var pedidoDto = new PedidoDto
        {
            NumeroPedido = numeroPedido,
            UsuarioId = UserId,
            Estado = "Pendiente",
            Subtotal = subtotal,
            Impuestos = impuestos,
            Total = subtotal + impuestos,
            MetodoPago = req.MetodoPago,
            DireccionEnvio = req.DireccionEnvio,
            Ciudad = req.Ciudad,
            TelefonoContacto = req.Telefono,
            Notas = req.Notas ?? ""
        };

        var pedidoSaved = await pedidoService.SaveDtoAsync(pedidoDto);
        if (pedidoSaved == null)
            return StatusCode(500, new { error = "Error creando el pedido." });

        foreach (var item in carritoItems)
        {
            var producto = await productoService.GetDtoById(item.ProductoId);
            var precio = producto?.PrecioFinal ?? 0;
            await pedidoDetalleService.SaveDtoAsync(new PedidoDetalleDto
            {
                PedidoId = pedidoSaved.Id,
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = precio,
                Subtotal = precio * item.Cantidad
            });
            await carritoService.DeleteHardDtoAsync(item.Id);
        }

        return CreatedAtAction(nameof(GetById), new { id = pedidoSaved.Id },
            new { pedidoSaved.Id, pedidoSaved.NumeroPedido, pedidoSaved.Total });
    }

    // ── ADMIN ──────────────────────────────────────────────────────────────────

    // GET api/pedidos/admin
    [Authorize(Roles = "Administrador")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetTodos([FromQuery] string? busqueda, [FromQuery] string? estado)
    {
        var todos = await pedidoService.GetAllListDto();

        if (!string.IsNullOrWhiteSpace(busqueda))
            todos = todos.Where(p =>
                p.NumeroPedido.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                p.UsuarioId.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                p.Estado.Contains(busqueda, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(estado))
            todos = todos.Where(p => p.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(todos.OrderByDescending(p => p.FechaPedido).Select(p => new
        {
            p.Id, p.NumeroPedido, p.UsuarioId, p.Estado,
            p.FechaPedido, p.Total
        }));
    }

    // GET api/pedidos/admin/{id}
    [Authorize(Roles = "Administrador")]
    [HttpGet("admin/{id:int}")]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var pedido = await pedidoService.GetPedidoWithDetailsAsync(id);
        return pedido == null ? NotFound() : Ok(pedido);
    }

    // PUT api/pedidos/admin/{id}/estado
    [Authorize(Roles = "Administrador")]
    [HttpPut("admin/{id:int}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoRequest req)
    {
        var estados = new[] { "Pendiente", "Confirmado", "EnProceso", "Enviado", "Entregado", "Cancelado" };
        if (!estados.Contains(req.Estado))
            return BadRequest(new { error = "Estado inválido." });

        var updated = await pedidoService.UpdateEstadoAsync(id, req.Estado);
        return updated == null ? NotFound() : Ok(updated);
    }
}

public record CrearPedidoRequest(string DireccionEnvio, string Ciudad, string Telefono, string MetodoPago, string? Notas);
public record ActualizarEstadoRequest(string Estado);
