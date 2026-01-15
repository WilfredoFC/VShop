using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Dtos.Categoria;
using VShop.Application.Interfaces;
using VShop.Application.ViewModels.Categoria;

namespace VShop.Controllers
{
    [Authorize(Roles = "Admin")]
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

        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _categoriaService.DeleteHardDtoAsync(id);
            return RedirectToAction("Index");
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Obtener categoria por id
            var categoria = (await _categoriaService.GetWithInclude([]))
                .FirstOrDefault(p => p.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            // Crear el ViewModel
            var viewModel = new CategoriaViewModel
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                FechaCreacion = categoria.FechaCreacion,
                EsActivo = categoria.EsActivo
            };

            return View(viewModel);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoriaViewModel model, string accion = null)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var categoriaQuery = await _categoriaService.GetWithInclude([]);
            var nombreExistente = categoriaQuery
                .Any(p => p.Nombre == model.Nombre && p.Id != model.Id);

            if (nombreExistente)
            {
                ModelState.AddModelError("Nombre", "El nombre de esta categoria ya existe. Por favor, ingrese un nombre único.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener la categoria actual
                    var categoria = await _categoriaService.GetDtoById(id);
                    if (categoria == null)
                    {
                        return NotFound();
                    }

                    // Actualizar propiedades
                    categoria.Nombre = model.Nombre;
                    categoria.Descripcion = model.Descripcion ?? "";
                    categoria.FechaActualizacion = DateTime.UtcNow;
                    categoria.EsActivo = model.EsActivo;

                    // Actualizar categoria
                    var result = await _categoriaService.UpdateDtoAsync(categoria, categoria.Id);

                    TempData["SuccessMessage"] = $"Categoria '{categoria.Nombre}' actualizada exitosamente.";

                    // Redireccionar según la acción
                    if (accion == "guardar-y-seguir")
                    {
                        return RedirectToAction(nameof(Edit), new { id = id });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar la categoria: {ex.Message}");
                }
            }

            return View(model);
        }
    }
}