using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VShop.Application.Dtos.Producto;
using VShop.Application.Dtos.ProductoImagen;
using VShop.Application.Interfaces;
using VShop.Application.ViewModels.Categoria;
using VShop.Application.ViewModels.Marca;
using VShop.Application.ViewModels.Producto;
using VShop.Application.ViewModels.ProductoImagen;
using VShop.Helpers;

namespace VShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductosController : Controller
    {
        private readonly IMarcaService _marcaService;
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly IProductoImagenService _productoImagenService;

        public ProductosController(IProductoService productoService, ICategoriaService categoriaService, IMarcaService marcaService, IProductoImagenService productoImagenService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _marcaService = marcaService;
            _productoImagenService = productoImagenService;
        }

        // GET: Productos
        public async Task<IActionResult> Index(ProductoFilterViewModel filtro, int pagina = 1, int registrosPorPagina = 10)
        {
            await _productoService.DesactivarProductosSinStockAsync();

            // Si el filtro viene nulo, inicializarlo
            filtro ??= new ProductoFilterViewModel
            {
                EstadoStock = "todos",
                OrdenarPor = "nombre",
                SoloConDescuento = false
            };

            // Consulta base
            var productosQuery = await _productoService.GetWithInclude(["Categorias", "Marcas", "Imagen"]);
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

            if (filtro.SoloConDescuento)
            {
                productosQuery = productosQuery.Where(p => p.PrecioDescuento.HasValue && p.PrecioDescuento < p.Precio).ToList();
            }

            // Filtro por estado de stock
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
                    SKU = p.SKU,
                    Stock = p.Stock,
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin categoria",
                    Marca = p.Marca != null ? p.Marca.Nombre : "Sin marca",
                    ImagenPrincipal = imgQuery.Where(img => img.ProductoId == p.Id && img.EsPrincipal)?.FirstOrDefault()?.UrlImagen ?? "",
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

        public async Task<IActionResult> Create()
        {
            var categoriasQuery = await _categoriaService.GetWithInclude(["Categorias", "Marcas", "Imagen"]);

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
            var marcasQuery = await _marcaService.GetWithInclude([]);

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

                    var result = await _productoService.SaveDtoAsync(producto);

                    if (result == null)
                    {
                        ModelState.AddModelError("db", "Error guardando el producto.");
                    }

                    // Guardar imágenes
                    if (model.Imagenes != null && model.Imagenes.Any())
                    {
                        await GuardarImagenesAsync(result.Id, model.Imagenes);
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

        private async Task GuardarImagenesAsync(int productoId, List<IFormFile> imagenes, bool isEditing = false)
        {
            int orden = 1;
            foreach (var imagen in imagenes)
            {
                if (imagen.Length > 0)
                {
                    var filename = FileManager.Upload(imagen, productoId.ToString(), "productos", isEditing);

                    var productoImagen = new ProductoImagenDto
                    {
                        ProductoId = productoId,
                        UrlImagen = filename ?? "",
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

        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _productoService.DeleteHardDtoAsync(id);
            if (producto == false)
            {
                ModelState.AddModelError("", $"Error al eliminar el producto");
            }
            
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> SoftDelete(int id)
        {
            var producto = await _productoService.CambiarEstadoProductoAsync(id);
            if (producto == false)
            {
                ModelState.AddModelError("", $"Error al desactivar el producto");
            }

            return RedirectToAction("Index");
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Obtener el producto con sus relaciones
            var producto = (await _productoService.GetWithInclude(["Categorias", "Marcas", "Imagenes"]))
                .FirstOrDefault(p => p.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            // Obtener categorías y marcas para los dropdowns
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

            // Obtener imágenes existentes del producto
            var imagenesExistentes = await _productoImagenService.GetByProductoIdAsync(producto.Id);

            // Crear el ViewModel
            var viewModel = new ProductoEditViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                PrecioDescuento = producto.PrecioDescuento,
                SKU = producto.SKU,
                Stock = producto.Stock,
                StockMinimo = producto.StockMinimo,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                EsActivo = producto.EsActivo,
                Categorias = categorias,
                Marcas = marcas,
                ImagenesExistentes = imagenesExistentes.Select(im => new ProductoImagenViewModel
                {
                    Id = im.Id,
                    UrlImagen = im.UrlImagen,
                    EsPrincipal = im.EsPrincipal,
                    Orden = im.Orden,
                    EsActivo = im.EsActivo
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductoEditViewModel model, string accion = null)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Verificar si el SKU ya existe, excluyendo el producto actual
            var productoQuery = await _productoService.GetWithInclude([]);
            var skuExistente = productoQuery
                .Any(p => p.SKU == model.SKU && p.Id != model.Id);

            if (skuExistente)
            {
                ModelState.AddModelError("SKU", "El SKU ya existe. Por favor, ingrese un SKU único.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el producto actual
                    var producto = await _productoService.GetDtoById(id);
                    if (producto == null)
                    {
                        return NotFound();
                    }

                    // Actualizar propiedades
                    producto.Nombre = model.Nombre;
                    producto.Descripcion = model.Descripcion ?? "";
                    producto.Precio = model.Precio;
                    producto.PrecioDescuento = model.PrecioDescuento;
                    producto.SKU = model.SKU;
                    producto.Stock = model.Stock;
                    producto.StockMinimo = model.StockMinimo;
                    producto.CategoriaId = model.CategoriaId;
                    producto.MarcaId = model.MarcaId;
                    producto.EsActivo = model.EsActivo;
                    producto.FechaActualizacion = DateTime.UtcNow;

                    // Actualizar producto
                    var result = await _productoService.UpdateDtoAsync(producto, producto.Id);

                    // Si se suben nuevas imágenes, guardarlas
                    if (model.Imagenes != null && model.Imagenes.Any())
                    {
                        await GuardarImagenesAsync(id, model.Imagenes);
                    }

                    // Si se especifica una imagen principal, actualizar
                    if (model.ImagenPrincipalId.HasValue)
                    {
                        await ActualizarImagenPrincipalAsync(id, model.ImagenPrincipalId.Value);
                    }

                    TempData["SuccessMessage"] = $"Producto '{producto.Nombre}' actualizado exitosamente.";

                    // Redireccionar según la acción
                    if (accion == "guardar-y-seguir")
                    {
                        return RedirectToAction(nameof(Edit), new { id = id });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar el producto: {ex.Message}");
                }
            }

            // Si hay errores, recargar las listas
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

            model.Categorias = categorias;
            model.Marcas = marcas;

            // Recargar las imágenes existentes
            var imagenesExistentes = await _productoImagenService.GetByProductoIdAsync(id);
            model.ImagenesExistentes = imagenesExistentes.Select(im => new ProductoImagenViewModel
            {
                Id = im.Id,
                UrlImagen = im.UrlImagen,
                EsPrincipal = im.EsPrincipal,
                Orden = im.Orden,
                EsActivo = im.EsActivo
            }).ToList();

            return View(model);
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Obtener el producto con sus relaciones
            var producto = (await _productoService.GetWithInclude(["Categorias", "Marcas", "Imagenes"]))
                .FirstOrDefault(p => p.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            // Obtener imágenes del producto
            var imagenes = await _productoImagenService.GetByProductoIdAsync(id);

            // Calcular si tiene descuento
            bool tieneDescuento = producto.PrecioDescuento.HasValue &&
                                 producto.PrecioDescuento < producto.Precio;

            decimal precioFinal = tieneDescuento ? producto.PrecioDescuento.Value : producto.Precio;

            // Determinar estado del stock
            string estadoStock;
            string estadoStockClase;

            if (producto.Stock == 0)
            {
                estadoStock = "Agotado";
                estadoStockClase = "danger";
            }
            else if (producto.Stock <= producto.StockMinimo)
            {
                estadoStock = "Stock Bajo";
                estadoStockClase = "warning";
            }
            else
            {
                estadoStock = "Disponible";
                estadoStockClase = "success";
            }

            // Crear ViewModel para detalles
            var viewModel = new ProductoDetailsViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                PrecioDescuento = producto.PrecioDescuento,
                PrecioFinal = precioFinal,
                TieneDescuento = tieneDescuento,
                SKU = producto.SKU,
                Stock = producto.Stock,
                StockMinimo = producto.StockMinimo,
                CategoriaId = producto.CategoriaId,
                Categoria = producto.Categoria?.Nombre,
                MarcaId = producto.MarcaId,
                Marca = producto.Marca?.Nombre ?? "Sin marca",
                EsActivo = producto.EsActivo,
                EstadoStock = estadoStock,
                EstadoStockClase = estadoStockClase,
                FechaCreacion = producto.FechaCreacion,
                FechaActualizacion = producto.FechaActualizacion,
                Imagenes = imagenes.Select(im => new ProductoImagenViewModel
                {
                    Id = im.Id,
                    UrlImagen = im.UrlImagen,
                    EsPrincipal = im.EsPrincipal,
                    Orden = im.Orden,
                    EsActivo = im.EsActivo
                }).ToList()
            };

            return View(viewModel);
        }

        // Métodos auxiliares
        private async Task ActualizarImagenPrincipalAsync(int productoId, int imagenId)
        {
            // Obtener todas las imágenes del producto
            var imagenes = await _productoImagenService.GetByProductoIdAsync(productoId);

            // Quitar la imagen principal actual
            foreach (var imagen in imagenes)
            {
                if (imagen.EsPrincipal && imagen.Id != imagenId)
                {
                    imagen.EsPrincipal = false;
                    await _productoImagenService.UpdateDtoAsync(imagen, imagen.Id);
                }
            }

            // Establecer la nueva imagen principal
            var nuevaPrincipal = imagenes.FirstOrDefault(im => im.Id == imagenId);
            if (nuevaPrincipal != null)
            {
                nuevaPrincipal.EsPrincipal = true;
                await _productoImagenService.UpdateDtoAsync(nuevaPrincipal, nuevaPrincipal.Id);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarImagen(int id)
        {
            var imagen = await _productoImagenService.GetDtoById(id);
            if (imagen != null)
            {
                // Eliminar físicamente el archivo si es necesario
                if (!string.IsNullOrEmpty(imagen.UrlImagen))
                {
                    FileManager.Delete(imagen.UrlImagen, "productos");
                }

                var resultado = await _productoImagenService.DeleteHardDtoAsync(id);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Imagen eliminada exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al eliminar la imagen.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
