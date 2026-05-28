using Microsoft.AspNetCore.Mvc;
using VShop.Application.Interfaces;

namespace VShop.Controllers
{
    public class ImagenesController : Controller
    {
        private readonly IProductoImagenService _productoImagenService;

        public ImagenesController(IProductoImagenService productoImagenService)
        {
            _productoImagenService = productoImagenService;
        }

        // GET /Imagenes/{id}
        [HttpGet("/Imagenes/{id:int}")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Imagen(int id)
        {
            var imagen = await _productoImagenService.GetDtoById(id);

            if (imagen?.Datos == null || imagen.Datos.Length == 0)
                return NotFound();

            return File(imagen.Datos, imagen.TipoContenido ?? "image/jpeg");
        }
    }
}
