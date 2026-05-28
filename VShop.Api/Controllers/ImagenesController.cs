using Microsoft.AspNetCore.Mvc;
using VShop.Application.Interfaces;

namespace VShop.Api.Controllers;

[ApiController]
public class ImagenesController(IProductoImagenService productoImagenService) : ControllerBase
{
    // GET /imagenes/{id}
    [HttpGet("/imagenes/{id:int}")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["id"])]
    public async Task<IActionResult> Imagen(int id)
    {
        var imagen = await productoImagenService.GetDtoById(id);
        if (imagen?.Datos == null || imagen.Datos.Length == 0)
            return NotFound();

        return File(imagen.Datos, imagen.TipoContenido ?? "image/jpeg");
    }
}
