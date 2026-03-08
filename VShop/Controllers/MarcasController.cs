using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Dtos.Marca;
using VShop.Application.Interfaces;
using VShop.Application.ViewModels.Marca;

namespace VShop.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class MarcasController : Controller
    {
        private readonly IMarcaService _marcaService;
        private readonly IMapper _mapper;

        public MarcasController(IMarcaService marcaService, IMapper mapper)
        {
            _marcaService = marcaService;
            _mapper = mapper;
        }

        // GET: Marcas
        public async Task<IActionResult> Index()
        {
            var marcas = await _marcaService.GetAllListDto();
            return View(_mapper.Map<List<MarcaViewModel>>(marcas));
        }

        // GET: Marcas/Create
        public async Task<IActionResult> Create()
        {
            return View(new MarcaViewModel());
        }

        // POST: Marcas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarcaViewModel model, string accion = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var marcaDto = new MarcaDto
                {
                    Nombre = model.Nombre.Trim(),
                    Descripcion = model.Descripcion?.Trim()
                };

                await _marcaService.SaveDtoAsync(marcaDto);

                TempData["SuccessMessage"] = $"Marca '{marcaDto.Nombre}' creada exitosamente.";

                if (accion == "guardar-y-nuevo")
                {
                    return RedirectToAction(nameof(Create));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear la marca: {ex.Message}");

                return View(model);
            }
        }
        public async Task<IActionResult> Delete(int id)
        {
            var marca = await _marcaService.DeleteHardDtoAsync(id);
            return RedirectToAction("Index");
        }

        // GET: Marcas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Obtener marca por id
            var marca = (await _marcaService.GetWithInclude([]))
                .FirstOrDefault(p => p.Id == id);

            if (marca == null)
            {
                return NotFound();
            }

            // Crear el ViewModel
            var viewModel = new MarcaViewModel
            {
                Id = marca.Id,
                Nombre = marca.Nombre,
                Descripcion = marca.Descripcion,
                EsActivo = marca.EsActivo,
                FechaCreacion = marca.FechaCreacion
            };

            return View(viewModel);
        }

        // POST: Marcas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MarcaViewModel model, string accion = null)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var marcaQuery = await _marcaService.GetWithInclude([]);
            var nombreExistente = marcaQuery
                .Any(p => p.Nombre == model.Nombre && p.Id != model.Id);

            if (nombreExistente)
            {
                ModelState.AddModelError("Nombre", "El nombre de esta marca ya existe. Por favor, ingrese un nombre único.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener la marca actual
                    var marca = await _marcaService.GetDtoById(id);
                    if (marca == null)
                    {
                        return NotFound();
                    }

                    // Actualizar propiedades
                    marca.Nombre = model.Nombre;
                    marca.Descripcion = model.Descripcion ?? "";
                    marca.FechaActualizacion = DateTime.UtcNow;
                    marca.EsActivo = model.EsActivo;


                    // Actualizar categoria
                    var result = await _marcaService.UpdateDtoAsync(marca, marca.Id);

                    TempData["SuccessMessage"] = $"Marca '{marca.Nombre}' actualizada exitosamente.";

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