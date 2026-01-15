using Microsoft.AspNetCore.Mvc;
using VShop.Application.Interfaces;
using VShop.Application.ViewModels.Categoria;
using VShop.Application.ViewModels.Marca;
using VShop.Application.ViewModels.Producto;
using VShop.Application.ViewModels.ProductoImagen;

namespace VShop.Controllers
{
    public class ClientController : Controller
    {
        private readonly IMarcaService _marcaService;
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly IProductoImagenService _productoImagenService;

        public ClientController(IProductoService productoService, ICategoriaService categoriaService, IMarcaService marcaService, IProductoImagenService productoImagenService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _marcaService = marcaService;
            _productoImagenService = productoImagenService;
        }
        // GET: Productos/Index
        public async Task<IActionResult> Index(ProductoFilterViewModel filtro, int pagina = 1, int registrosPorPagina = 12)
        {
            await LoadCategorias();
            // Consulta base - solo productos activos
            var productosQuery = (await _productoService.GetWithInclude(["Categorias", "Marcas", "Imagen"]))
                .Where(p => p.EsActivo && p.Stock > 0) // Solo productos activos y con stock
                .ToList();

            var imgQuery = await _productoImagenService.GetWithInclude([]);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(filtro.Busqueda))
            {
                productosQuery = productosQuery.Where(p =>
                    (p.Nombre != null && p.Nombre.Contains(filtro.Busqueda, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Descripcion != null && p.Descripcion.Contains(filtro.Busqueda, StringComparison.OrdinalIgnoreCase)) ||
                    (p.SKU != null && p.SKU.Contains(filtro.Busqueda, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (filtro.CategoriaId.HasValue)
            {
                productosQuery = productosQuery.Where(p => p.CategoriaId == filtro.CategoriaId.Value).ToList();
            }

            if (filtro.MarcaId.HasValue)
            {
                productosQuery = productosQuery.Where(p => p.MarcaId == filtro.MarcaId.Value).ToList();
            }

            // Filtro por estado de stock para clientes
            switch (filtro.EstadoStock)
            {
                case "disponible":
                    productosQuery = productosQuery.Where(p => p.Stock > 10).ToList();
                    break;
                case "bajo":
                    productosQuery = productosQuery.Where(p => p.Stock > 0 && p.Stock <= 10).ToList();
                    break;
                case "agotado":
                    productosQuery = productosQuery.Where(p => p.Stock == 0).ToList();
                    break;
            }

            if (filtro.SoloConDescuento)
            {
                productosQuery = productosQuery.Where(p => p.PrecioDescuento.HasValue && p.PrecioDescuento < p.Precio).ToList();
            }

            // Ordenar
            switch (filtro.OrdenarPor)
            {
                case "precio_asc":
                    productosQuery = productosQuery.OrderBy(p => p.Precio).ToList();
                    break;
                case "precio_desc":
                    productosQuery = productosQuery.OrderByDescending(p => p.Precio).ToList();
                    break;
                case "stock":
                    productosQuery = productosQuery.OrderBy(p => p.Stock).ToList();
                    break;
                case "reciente":
                    productosQuery = productosQuery.OrderByDescending(p => p.FechaCreacion).ToList();
                    break;
                default:
                    productosQuery = productosQuery.OrderBy(p => p.Nombre).ToList();
                    break;
            }

            // Paginación
            var totalRegistros = productosQuery.Count();
            var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            var productos = productosQuery
                            .Skip((pagina - 1) * registrosPorPagina)
                            .Take(registrosPorPagina)
                            .Select(p => new ProductoViewModel
                            {
                                Id = p.Id,
                                Nombre = p.Nombre,
                                Descripcion = p.Descripcion,
                                Precio = p.Precio,
                                PrecioDescuento = p.PrecioDescuento,
                                SKU = p.SKU, // Mantenemos el SKU en el modelo pero no lo mostramos en la vista
                                Stock = p.Stock,
                                Categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin categoría",
                                Marca = p.Marca != null ? p.Marca.Nombre : "Sin marca",
                                ImagenPrincipal = imgQuery.Where(img => img.ProductoId == p.Id && img.EsPrincipal)
                                                         .FirstOrDefault()?.UrlImagen ?? "",
                                EsActivo = p.EsActivo
                            })
                            .ToList();

            // Obtener categorías y marcas para los filtros (solo activas)
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
                Filtro = filtro,
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

        // GET: Productos/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            await LoadCategorias();
            var producto = (await _productoService.GetWithInclude(["Categorias", "Marcas", "Imagenes"]))
                .FirstOrDefault(p => p.Id == id && p.EsActivo);

            if (producto == null)
            {
                return NotFound();
            }

            // Obtener imágenes del producto
            var imagenes = await _productoImagenService.GetByProductoIdAsync(id);

            // Obtener productos relacionados (misma categoría)
            var productosRelacionados = (await _productoService.GetWithInclude(["Categorias", "Marcas", "Imagen"]))
                .Where(p => p.Id != id &&
                            p.CategoriaId == producto.CategoriaId &&
                            p.EsActivo)
                .Take(4)
                .ToList();

            var imgQuery = await _productoImagenService.GetWithInclude([]);

            // Crear ViewModel usando ProductosIndexViewModel
            var productoViewModel = new ProductoViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                PrecioDescuento = producto.PrecioDescuento,
                SKU = producto.SKU,
                Stock = producto.Stock,
                Categoria = producto.Categoria?.Nombre ?? "Sin categoría",
                Marca = producto.Marca?.Nombre ?? "Sin marca",
                ImagenPrincipal = imagenes.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen ?? "",
                EsActivo = producto.EsActivo
            };

            var productosRelacionadosViewModels = productosRelacionados.Select(p => new ProductoViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre,
                ImagenPrincipal = imgQuery.Where(img => img.ProductoId == p.Id && img.EsPrincipal)
                                         .FirstOrDefault()?.UrlImagen ?? "",
                EsActivo = p.EsActivo
            }).ToList();

            // Crear ViewModel para detalles
            var viewModel = new ProductoDetalleClienteViewModel
            {
                Producto = productoViewModel,
                Imagenes = imagenes.Select(im => new ProductoImagenViewModel
                {
                    Id = im.Id,
                    UrlImagen = im.UrlImagen,
                    EsPrincipal = im.EsPrincipal
                }).ToList(),
                ProductosRelacionados = productosRelacionadosViewModels
            };

            return View(viewModel);
        }
        // En el controlador, agrega este método para las categorías
        private async Task LoadCategorias()
        {
            var categoriasQuery = await _categoriaService.GetWithInclude([]);
            var categorias = categoriasQuery
                .Where(c => c.EsActivo)
                .Select(c => new CategoriaViewModel { Id = c.Id, Nombre = c.Nombre })
                .ToList();

            ViewBag.Categorias = categorias;
        }

        // Método para sugerencias de búsqueda
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            var productos = (await _productoService.GetWithInclude(["Categorias"]))
                .Where(p => p.EsActivo &&
                       (p.Nombre.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        p.Descripcion.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Take(5)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Nombre,
                    category = p.Categoria?.Nombre
                })
                .ToList();

            return Json(productos);
        }

        // Método para obtener categorías activas (para AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCategoriasActivas()
        {
            var categoriasQuery = await _categoriaService.GetWithInclude([]);
            var categorias = categoriasQuery
                .Where(c => c.EsActivo)
                .Select(c => new { id = c.Id, nombre = c.Nombre })
                .ToList();

            return Json(categorias);
        }
    }
}
