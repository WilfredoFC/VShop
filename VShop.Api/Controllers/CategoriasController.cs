using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Dtos.Categoria;
using VShop.Application.Interfaces;

namespace VShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
{
    // GET api/categorias — solo activas (público)
    [HttpGet]
    public async Task<IActionResult> GetActivas()
    {
        var todas = await categoriaService.GetWithInclude([]);
        return Ok(todas.Where(c => c.EsActivo).Select(c => new { c.Id, c.Nombre, c.Descripcion, c.Tipo }));
    }

    // GET api/categorias/all — todas (admin)
    [Authorize(Roles = "Administrador")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var todas = await categoriaService.GetAllListDto();
        return Ok(todas.Select(c => new { c.Id, c.Nombre, c.Descripcion, c.Tipo, c.EsActivo }));
    }

    // GET api/categorias/{id}
    [Authorize(Roles = "Administrador")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await categoriaService.GetDtoById(id);
        return c == null ? NotFound() : Ok(c);
    }

    // POST api/categorias
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoriaRequest req)
    {
        var existing = await categoriaService.GetWithInclude([]);
        if (existing.Any(c => c.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { error = "Ya existe una categoría con ese nombre." });

        var saved = await categoriaService.SaveDtoAsync(new CategoriaDto
        {
            Nombre = req.Nombre,
            Descripcion = req.Descripcion,
            Tipo = req.Tipo,
            EsActivo = true
        });
        return CreatedAtAction(nameof(GetById), new { id = saved?.Id }, saved);
    }

    // PUT api/categorias/{id}
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaRequest req)
    {
        var existing = await categoriaService.GetWithInclude([]);
        if (existing.Any(c => c.Id != id && c.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { error = "Ya existe una categoría con ese nombre." });

        var dto = new CategoriaDto { Nombre = req.Nombre, Descripcion = req.Descripcion, Tipo = req.Tipo };
        var updated = await categoriaService.UpdateDtoAsync(dto, id);
        return updated == null ? NotFound() : Ok(updated);
    }

    // DELETE api/categorias/{id}
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await categoriaService.DeleteHardDtoAsync(id);
        return NoContent();
    }
}

public record CategoriaRequest(string Nombre, string Descripcion, string Tipo);
