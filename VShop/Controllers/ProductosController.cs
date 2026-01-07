using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VShop.Application.Dtos.Producto;
using VShop.Application.Dtos.ProductoImagen;
using VShop.Application.Interfaces;
using VShop.Application.ViewModels.Marca;
using VShop.Application.ViewModels.Productos;
using VShop.Helpers;

namespace VShop.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly IMarcaService _marcaService;
        private readonly IProductoImagenService _productoImagenService;

        public ProductosController(IProductoService productoService, ICategoriaService categoriaService, IMarcaService marcaService, IProductoImagenService productoImagenService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _marcaService = marcaService;
            _productoImagenService = productoImagenService;
        }

        // GET: Productos
        public async Task<IActionResult> Index(
            string busqueda = null,
            int? categoriaId = null,
            int? marcaId = null,
            string estadoStock = "todos",
            string ordenarPor = "nombre",
            bool soloConDescuento = false,
            int pagina = 1,
            int registrosPorPagina = 10)
        {
            // Consulta base
            var query = await _productoService.GetWithInclude(["Categoria", "Marca", "Imagen"]);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    p.Descripcion.Contains(busqueda) ||
                    p.SKU.Contains(busqueda)).ToList();
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value).ToList();
            }

            if (marcaId.HasValue)
            {
                query = query.Where(p => p.MarcaId == marcaId.Value).ToList();
            }

            if (soloConDescuento)
            {
                query = query.Where(p => p.PrecioDescuento.HasValue && p.PrecioDescuento < p.Precio).ToList();
            }

            // Filtro por estado de stock
            switch (estadoStock)
            {
                case "disponible":
                    query = query.Where(p => p.Stock > 10).ToList();
                    break;
                case "bajo":
                    query = query.Where(p => p.Stock > 0 && p.Stock <= 10).ToList();
                    break;
                case "agotado":
                    query = query.Where(p => p.Stock == 0).ToList();
                    break;
            }

            // Ordenar
            switch (ordenarPor)
            {
                case "precio_asc":
                    query = query.OrderBy(p => p.Precio).ToList();
                    break;
                case "precio_desc":
                    query = query.OrderByDescending(p => p.Precio).ToList();
                    break;
                case "stock":
                    query = query.OrderBy(p => p.Stock).ToList();
                    break;
                case "reciente":
                    query = query.OrderByDescending(p => p.FechaCreacion).ToList();
                    break;
                default:
                    query = query.OrderBy(p => p.Nombre).ToList();
                    break;
            }

            // Paginación
            var totalRegistros = query.Count();
            var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            var productos = query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(p => new ProductoViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    PrecioDescuento = p.PrecioDescuento,
                    SKU = p.SKU,
                    Stock = p.Stock,
                    Categoria = p.Categoria.Nombre,
                    Marca = p.Marca != null ? p.Marca.Nombre : "Sin marca",
                    ImagenPrincipal = p.Imagenes.FirstOrDefault().UrlImagen,
                    EsActivo = p.EsActivo
                })
                .ToList();

            // Obtener categorías y marcas para los filtros
            var categoriasQuery = await _categoriaService.GetWithInclude([]);

            var categorias = categoriasQuery
                .Where(c => c.EsActivo)
                .Select(c => new CategoriaViewModel { Id = c.Id, Nombre = c.Nombre })
                .ToList();

            var marcasQuery = await _marcaService.GetWithInclude([]);
            var marcas = marcasQuery
                .Where(m => m.EsActivo)
                .Select(m => new MarcaViewModel { Id = m.Id, Nombre = m.Nombre })
                .ToList();

            // Crear ViewModel
            var viewModel = new ProductosIndexViewModel
            {
                Productos = productos,
                Categorias = categorias,
                Marcas = marcas,
                Filtro = new ProductoFilterViewModel
                {
                    Busqueda = busqueda,
                    CategoriaId = categoriaId,
                    MarcaId = marcaId,
                    EstadoStock = estadoStock,
                    OrdenarPor = ordenarPor,
                    SoloConDescuento = soloConDescuento
                },
                Paginacion = new PaginacionViewModel
                {
                    PaginaActual = pagina,
                    TotalPaginas = totalPaginas,
                    TotalRegistros = totalRegistros,
                    RegistrosPorPagina = registrosPorPagina
                }
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var categoriasQuery = await _categoriaService.GetWithInclude([]);
            var categorias = categoriasQuery
                .Where(c => c.EsActivo)
                .Select(c => new CategoriaViewModel { Id = c.Id, Nombre = c.Nombre })
                .ToList();

            var marcasQuery = await _marcaService.GetWithInclude([]);
            var marcas = marcasQuery
                .Where(m => m.EsActivo)
                .Select(m => new MarcaViewModel { Id = m.Id, Nombre = m.Nombre })
                .ToList();

            var viewModel = new ProductoCreateViewModel
            {
                Categorias = categorias,
                Marcas = marcas,
                EsActivo = true,
                EsNuevo = true,
                StockMinimo = 10
            };

            return View(viewModel);
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoCreateViewModel model, string accion = null)
        {
            // Verificar si el SKU ya existe
            var productoQuery = await _productoService.GetWithInclude([]);
            var categoriasQuery = await _categoriaService.GetWithInclude([]);
            var marcasQuery = await _marcaService.GetWithInclude ([]);

            var skuExistente = productoQuery
                .Where(p => p.SKU == model.SKU).Any();

            if (skuExistente)
            {
                ModelState.AddModelError("SKU", "El SKU ya existe. Por favor, ingrese un SKU único.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var producto = new ProductoDto
                    {
                        Nombre = model.Nombre,
                        Descripcion = model.Descripcion,
                        Precio = model.Precio,
                        PrecioDescuento = model.PrecioDescuento,
                        SKU = model.SKU,
                        Stock = model.Stock,
                        StockMinimo = model.StockMinimo,
                        CategoriaId = model.CategoriaId,
                        MarcaId = model.MarcaId,
                        EsActivo = model.EsActivo,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    };

                    await _productoService.SaveDtoAsync(producto);

                    // Guardar imágenes
                    if (model.Imagenes != null && model.Imagenes.Any())
                    {
                        await GuardarImagenesAsync(producto.Id, model.Imagenes);
                    }

                    TempData["SuccessMessage"] = $"Producto '{producto.Nombre}' creado exitosamente.";

                    // Redireccionar según la acción
                    if (accion == "guardar-y-nuevo")
                    {
                        return RedirectToAction(nameof(Create));
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al crear el producto: {ex.Message}");
                }
            }

            // Si hay errores, recargar las listas
            model.Categorias = categoriasQuery
                .Where(c => c.EsActivo)
                .Select(c => new CategoriaViewModel { Id = c.Id, Nombre = c.Nombre })
                .ToList();

            model.Marcas = marcasQuery
                .Where(m => m.EsActivo)
                .Select(m => new MarcaViewModel { Id = m.Id, Nombre = m.Nombre })
                .ToList();

            return View(model);
        }

        private async Task GuardarImagenesAsync(int productoId, List<IFormFile> imagenes)
        {
            int orden = 1;
            foreach (var imagen in imagenes)
            {
                if (imagen.Length > 0)
                {
                    var filename = FileManager.Upload(imagen, productoId.ToString(), "productos");

                    var productoImagen = new ProductoImagenDto
                    {
                        ProductoId = productoId,
                        UrlImagen = filename,
                        EsPrincipal = orden == 1,
                        Orden = orden,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow,
                        EsActivo = true
                    };

                    await _productoImagenService.SaveDtoAsync(productoImagen);
                    orden++;
                }
            }
        }
    }
}
