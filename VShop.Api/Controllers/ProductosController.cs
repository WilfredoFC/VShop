using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Dtos.Producto;
using VShop.Application.Dtos.ProductoImagen;
using VShop.Application.Interfaces;

namespace VShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController(
    IProductoService productoService,
    IProductoImagenService productoImagenService,
    ICategoriaService categoriaService,
    IMarcaService marcaService) : ControllerBase
{
    // GET api/productos?busqueda=&categoriaId=&marcaId=&soloConStock=&soloConDescuento=&ordenar=&pagina=&por=
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? busqueda,
        [FromQuery] int? categoriaId,
        [FromQuery] int? marcaId,
        [FromQuery] bool soloConStock = true,
        [FromQuery] bool soloConDescuento = false,
        [FromQuery] string ordenar = "nombre",
        [FromQuery] int pagina = 1,
        [FromQuery] int por = 12)
    {
        var todos = await productoService.GetWithInclude(["Categorias", "Marcas", "Imagenes"]);

        var query = todos.Where(p => p.EsActivo);

        if (soloConStock) query = query.Where(p => p.Stock > 0);
        if (soloConDescuento) query = query.Where(p => p.TieneDescuento);
        if (categoriaId.HasValue) query = query.Where(p => p.CategoriaId == categoriaId);
        if (marcaId.HasValue) query = query.Where(p => p.MarcaId == marcaId);
        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p =>
                p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                p.Descripcion.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                (p.SKU?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false));

        query = ordenar switch
        {
            "precio_asc" => query.OrderBy(p => p.PrecioFinal),
            "precio_desc" => query.OrderByDescending(p => p.PrecioFinal),
            "stock" => query.OrderByDescending(p => p.Stock),
            "reciente" => query.OrderByDescending(p => p.FechaCreacion),
            _ => query.OrderBy(p => p.Nombre)
        };

        var total = query.Count();
        var items = query.Skip((pagina - 1) * por).Take(por)
            .Select(p => new
            {
                p.Id, p.Nombre, p.Descripcion, p.Precio, p.PrecioDescuento,
                p.Stock, p.TieneDescuento, p.PrecioFinal,
                Categoria = p.Categoria?.Nombre ?? "Sin categoría",
                Marca = p.Marca?.Nombre ?? "Sin marca",
                p.CategoriaId, p.MarcaId, p.EsActivo,
                ImagenPrincipalId = p.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.Id
                    ?? p.Imagenes?.FirstOrDefault()?.Id
            }).ToList();

        return Ok(new
        {
            Items = items,
            TotalRegistros = total,
            PaginaActual = pagina,
            TotalPaginas = (int)Math.Ceiling(total / (double)por)
        });
    }

    // GET api/productos/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var todos = await productoService.GetWithInclude(["Categorias", "Marcas", "Imagenes"]);
        var p = todos.FirstOrDefault(x => x.Id == id && x.EsActivo);
        if (p == null) return NotFound();

        var imagenes = await productoImagenService.GetByProductoIdAsync(id);

        return Ok(new
        {
            p.Id, p.Nombre, p.Descripcion, p.Precio, p.PrecioDescuento,
            p.SKU, p.Stock, p.TieneDescuento, p.PrecioFinal, p.EsActivo,
            Categoria = p.Categoria?.Nombre ?? "Sin categoría",
            Marca = p.Marca?.Nombre ?? "Sin marca",
            p.CategoriaId, p.MarcaId,
            Imagenes = imagenes.Select(i => new { i.Id, i.EsPrincipal, i.Orden })
        });
    }

    // GET api/productos/{id}/relacionados
    [HttpGet("{id:int}/relacionados")]
    public async Task<IActionResult> GetRelacionados(int id)
    {
        var todos = await productoService.GetWithInclude(["Categorias", "Imagenes"]);
        var producto = todos.FirstOrDefault(p => p.Id == id);
        if (producto == null) return NotFound();

        var imgQuery = await productoImagenService.GetWithInclude([]);

        var relacionados = todos
            .Where(p => p.Id != id && p.CategoriaId == producto.CategoriaId && p.EsActivo && p.Stock > 0)
            .Take(4)
            .Select(p => new
            {
                p.Id, p.Nombre, p.Precio, p.PrecioDescuento, p.TieneDescuento,
                ImagenPrincipalId = imgQuery.Where(i => i.ProductoId == p.Id && i.EsPrincipal).FirstOrDefault()?.Id
            });

        return Ok(relacionados);
    }

    // GET api/productos/buscar?q= (sugerencias)
    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarSugerencias([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<object>());

        var todos = await productoService.GetWithInclude([]);
        var sugerencias = todos
            .Where(p => p.EsActivo && p.Stock > 0 &&
                p.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .Select(p => new { p.Id, p.Nombre, p.Precio, p.TieneDescuento, p.PrecioFinal });

        return Ok(sugerencias);
    }

    // ── ADMIN ──────────────────────────────────────────────────────────────────

    // POST api/productos
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductoRequest req)
    {
        var dto = new ProductoDto
        {
            Nombre = req.Nombre,
            Descripcion = req.Descripcion,
            Precio = req.Precio,
            PrecioDescuento = req.PrecioDescuento,
            SKU = req.SKU,
            Stock = req.Stock,
            StockMinimo = req.StockMinimo,
            CategoriaId = req.CategoriaId,
            MarcaId = req.MarcaId,
            EsActivo = req.EsActivo
        };
        var saved = await productoService.SaveDtoAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = saved?.Id }, saved);
    }

    // PUT api/productos/{id}
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductoRequest req)
    {
        var dto = new ProductoDto
        {
            Nombre = req.Nombre,
            Descripcion = req.Descripcion,
            Precio = req.Precio,
            PrecioDescuento = req.PrecioDescuento,
            SKU = req.SKU,
            Stock = req.Stock,
            StockMinimo = req.StockMinimo,
            CategoriaId = req.CategoriaId,
            MarcaId = req.MarcaId,
            EsActivo = req.EsActivo
        };
        var updated = await productoService.UpdateDtoAsync(dto, id);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    // DELETE api/productos/{id}
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await productoService.DeleteHardDtoAsync(id);
        return NoContent();
    }

    // POST api/productos/{id}/imagenes
    [Authorize(Roles = "Administrador")]
    [HttpPost("{id:int}/imagenes")]
    public async Task<IActionResult> SubirImagenes(int id, [FromForm] IFormFileCollection imagenes)
    {
        if (imagenes.Count == 0) return BadRequest(new { error = "Sin imágenes." });

        var existentes = (await productoImagenService.GetByProductoIdAsync(id)).ToList();
        var hayPrincipal = existentes.Any(i => i.EsPrincipal);

        for (int i = 0; i < imagenes.Count; i++)
        {
            var img = imagenes[i];
            using var ms = new MemoryStream();
            await img.CopyToAsync(ms);

            await productoImagenService.SaveDtoAsync(new ProductoImagenDto
            {
                ProductoId = id,
                Datos = ms.ToArray(),
                TipoContenido = img.ContentType,
                EsPrincipal = !hayPrincipal && i == 0,
                Orden = existentes.Count + i,
                EsActivo = true
            });
        }

        var resultado = await productoImagenService.GetByProductoIdAsync(id);
        return Ok(resultado.Select(i => new { i.Id, i.EsPrincipal, i.Orden }));
    }

    // DELETE api/productos/imagenes/{imagenId}
    [Authorize(Roles = "Administrador")]
    [HttpDelete("imagenes/{imagenId:int}")]
    public async Task<IActionResult> EliminarImagen(int imagenId)
    {
        await productoImagenService.DeleteHardDtoAsync(imagenId);
        return NoContent();
    }

    // PATCH api/productos/imagenes/{imagenId}/principal
    [Authorize(Roles = "Administrador")]
    [HttpPatch("imagenes/{imagenId:int}/principal")]
    public async Task<IActionResult> SetPrincipal(int imagenId)
    {
        var imagen = await productoImagenService.GetDtoById(imagenId);
        if (imagen == null) return NotFound();

        var todas = (await productoImagenService.GetByProductoIdAsync(imagen.ProductoId)).ToList();
        foreach (var img in todas)
        {
            img.EsPrincipal = img.Id == imagenId;
            await productoImagenService.UpdateDtoAsync(img, img.Id);
        }
        return NoContent();
    }

    // GET api/productos/admin
    [Authorize(Roles = "Administrador")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin(
        [FromQuery] string? busqueda,
        [FromQuery] int pagina = 1,
        [FromQuery] int por = 20)
    {
        var todos = await productoService.GetWithInclude(["Categorias", "Marcas"]);
        var imgQuery = await productoImagenService.GetWithInclude([]);

        if (!string.IsNullOrWhiteSpace(busqueda))
            todos = todos.Where(p =>
                p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                p.SKU.Contains(busqueda, StringComparison.OrdinalIgnoreCase)).ToList();

        var total = todos.Count;
        var items = todos.OrderByDescending(p => p.FechaCreacion)
            .Skip((pagina - 1) * por).Take(por)
            .Select(p => new
            {
                p.Id, p.Nombre, p.SKU, p.Precio, p.PrecioDescuento,
                p.Stock, p.StockMinimo, p.EsActivo, p.TieneDescuento,
                Categoria = p.Categoria?.Nombre ?? "-",
                Marca = p.Marca?.Nombre ?? "-",
                p.CategoriaId, p.MarcaId,
                ImagenPrincipalId = imgQuery.Where(i => i.ProductoId == p.Id && i.EsPrincipal).FirstOrDefault()?.Id
            }).ToList();

        return Ok(new { Items = items, TotalRegistros = total, PaginaActual = pagina, TotalPaginas = (int)Math.Ceiling(total / (double)por) });
    }
}

public record CreateProductoRequest(
    string Nombre, string Descripcion, decimal Precio, decimal? PrecioDescuento,
    string SKU, int Stock, int StockMinimo, int CategoriaId, int? MarcaId, bool EsActivo);
