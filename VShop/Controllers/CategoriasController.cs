using Microsoft.AspNetCore.Mvc;
using VShop.Application.Dtos.Categoria;
using VShop.Application.Interfaces;
using VShop.Application.ViewModels.Categoria;

namespace VShop.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            var categorias = await _categoriaService.GetAllListDto();
            return View(categorias);
        }

        // GET: Categorias/Create
        public async Task<IActionResult> Create()
        {
            var categoriasPadre = await _categoriaService.GetAllListDto();

            var viewModel = new CategoriaCreateViewModel
            {
                EsActivo = true,
                MostrarEnMenu = true,
                Orden = 0,
                Tipo = "Personal"
            };

            return View(viewModel);
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaCreateViewModel model, string accion = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var categoriaDto = new CategoriaDto
                {
                    Nombre = model.Nombre.Trim(),
                    Descripcion = model.Descripcion?.Trim(),
                    Tipo = model.Tipo,
                    EsActivo = model.EsActivo,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };

                await _categoriaService.SaveDtoAsync(categoriaDto);

                TempData["SuccessMessage"] = $"Categoría '{categoriaDto.Nombre}' creada exitosamente.";

                if (accion == "guardar-y-nuevo")
                {
                    return RedirectToAction(nameof(Create));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear la categoría: {ex.Message}");

                return View(model);
            }
        }
    }
}