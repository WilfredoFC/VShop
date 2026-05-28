using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Dtos.Marca;
using VShop.Application.Interfaces;

namespace VShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcasController(IMarcaService marcaService) : ControllerBase
{
    // GET api/marcas — solo activas (público)
    [HttpGet]
    public async Task<IActionResult> GetActivas()
    {
        var todas = await marcaService.GetWithInclude([]);
        return Ok(todas.Where(m => m.EsActivo).Select(m => new { m.Id, m.Nombre, m.EsActivo }));
    }

    // GET api/marcas/all — todas (admin)
    [Authorize(Roles = "Administrador")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var todas = await marcaService.GetAllListDto();
        return Ok(todas.Select(m => new { m.Id, m.Nombre, m.EsActivo }));
    }

    // GET api/marcas/{id}
    [Authorize(Roles = "Administrador")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var m = await marcaService.GetDtoById(id);
        return m == null ? NotFound() : Ok(m);
    }

    // POST api/marcas
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MarcaRequest req)
    {
        var existing = await marcaService.GetWithInclude([]);
        if (existing.Any(m => m.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { error = "Ya existe una marca con ese nombre." });

        var saved = await marcaService.SaveDtoAsync(new MarcaDto { Nombre = req.Nombre, EsActivo = true });
        return CreatedAtAction(nameof(GetById), new { id = saved?.Id }, saved);
    }

    // PUT api/marcas/{id}
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MarcaRequest req)
    {
        var existing = await marcaService.GetWithInclude([]);
        if (existing.Any(m => m.Id != id && m.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { error = "Ya existe una marca con ese nombre." });

        var updated = await marcaService.UpdateDtoAsync(new MarcaDto { Nombre = req.Nombre }, id);
        return updated == null ? NotFound() : Ok(updated);
    }

    // DELETE api/marcas/{id}
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await marcaService.DeleteHardDtoAsync(id);
        return NoContent();
    }
}

public record MarcaRequest(string Nombre);
