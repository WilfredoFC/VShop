using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Dtos.CarritoItem;
using VShop.Application.Interfaces;

namespace VShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarritoController(
    ICarritoItemService carritoService,
    IProductoService productoService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET api/carrito
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await carritoService.GetDtoByUserId(UserId);
        var enriched = items.Select(i => new
        {
            i.Id, i.ProductoId, i.Cantidad,
            NombreProducto = i.Producto?.Nombre ?? "-",
            PrecioUnitario = i.Producto?.PrecioFinal ?? 0,
            Stock = i.Producto?.Stock ?? 0,
            ImagenId = i.Producto?.Imagenes?.FirstOrDefault(im => im.EsPrincipal)?.Id
                    ?? i.Producto?.Imagenes?.FirstOrDefault()?.Id,
            Subtotal = (i.Producto?.PrecioFinal ?? 0) * i.Cantidad
        }).ToList();

        var subtotal = enriched.Sum(i => i.Subtotal);
        var taxRate = 0.18m;
        var impuestos = subtotal * taxRate;

        return Ok(new
        {
            Items = enriched,
            Subtotal = subtotal,
            Impuestos = impuestos,
            Total = subtotal + impuestos,
            CantidadItems = enriched.Sum(i => i.Cantidad)
        });
    }

    // POST api/carrito
    [HttpPost]
    public async Task<IActionResult> Agregar([FromBody] AgregarItemRequest req)
    {
        var producto = await productoService.GetDtoById(req.ProductoId);
        if (producto == null) return NotFound(new { error = "Producto no encontrado." });
        if (!producto.EsActivo || producto.Stock <= 0)
            return BadRequest(new { error = "Producto no disponible." });

        var existing = (await carritoService.GetDtoByUserId(UserId))
            .FirstOrDefault(i => i.ProductoId == req.ProductoId);

        var cantidadTotal = (existing?.Cantidad ?? 0) + req.Cantidad;
        if (cantidadTotal > producto.Stock)
            return BadRequest(new { error = $"Stock insuficiente. Disponible: {producto.Stock}" });

        if (existing != null)
        {
            existing.Cantidad = cantidadTotal;
            await carritoService.UpdateDtoAsync(existing, existing.Id);
        }
        else
        {
            await carritoService.SaveDtoAsync(new CarritoItemDto
            {
                UsuarioId = UserId,
                ProductoId = req.ProductoId,
                Cantidad = req.Cantidad
            });
        }

        return Ok(new { message = "Producto agregado al carrito." });
    }

    // PUT api/carrito/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarCantidad(int id, [FromBody] ActualizarCantidadRequest req)
    {
        var item = await carritoService.GetDtoById(id);
        if (item == null || item.UsuarioId != UserId) return NotFound();

        var producto = await productoService.GetDtoById(item.ProductoId);
        if (req.NuevaCantidad > (producto?.Stock ?? 0))
            return BadRequest(new { error = $"Stock insuficiente. Disponible: {producto?.Stock}" });

        if (req.NuevaCantidad <= 0)
        {
            await carritoService.DeleteHardDtoAsync(id);
            return Ok(new { message = "Item eliminado." });
        }

        item.Cantidad = req.NuevaCantidad;
        await carritoService.UpdateDtoAsync(item, id);
        return Ok(new { message = "Cantidad actualizada." });
    }

    // DELETE api/carrito/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var item = await carritoService.GetDtoById(id);
        if (item == null || item.UsuarioId != UserId) return NotFound();

        await carritoService.DeleteHardDtoAsync(id);
        return NoContent();
    }

    // DELETE api/carrito
    [HttpDelete]
    public async Task<IActionResult> Vaciar()
    {
        var items = await carritoService.GetDtoByUserId(UserId);
        foreach (var item in items)
            await carritoService.DeleteHardDtoAsync(item.Id);

        return NoContent();
    }
}

public record AgregarItemRequest(int ProductoId, int Cantidad);
public record ActualizarCantidadRequest(int NuevaCantidad);
