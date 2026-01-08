using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Dtos.Marca;
using VShop.Application.Interfaces;
using VShop.Application.ViewModels.Marca;

namespace VShop.Controllers
{
    public class MarcasController : Controller
    {
        private readonly IMarcaService _marcaService;
        private readonly IMapper _mapper;

        public MarcasController(IMarcaService marcaService, IMapper mapper)
        {
            _marcaService = marcaService;
            _mapper = mapper;
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            var marcas = await _marcaService.GetAllListDto();
            return View(_mapper.Map<List<MarcaViewModel>>(marcas));
        }

        // GET: Categorias/Create
        public async Task<IActionResult> Create()
        {
            return View(new MarcaViewModel());
        }

        // POST: Categorias/Create
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
    }
}