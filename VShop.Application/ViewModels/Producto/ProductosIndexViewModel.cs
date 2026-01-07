using VShop.Application.ViewModels.Marca;

namespace VShop.Application.ViewModels.Productos
{
        public class ProductosIndexViewModel
        {
            public List<ProductoViewModel> Productos { get; set; } = new();
            public ProductoFilterViewModel Filtro { get; set; } = new();
            public PaginacionViewModel Paginacion { get; set; } = new();
            public List<CategoriaViewModel> Categorias { get; set; } = new();
            public List<MarcaViewModel> Marcas { get; set; } = new();
        }

    }