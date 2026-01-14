using VShop.Application.ViewModels.ProductoImagen;

namespace VShop.Application.ViewModels.Producto
{
    public class ProductoDetalleClienteViewModel
    {
        public ProductoViewModel Producto { get; set; }
        public List<ProductoImagenViewModel> Imagenes { get; set; } = new List<ProductoImagenViewModel>();
        public List<ProductoViewModel> ProductosRelacionados { get; set; } = new List<ProductoViewModel>();
    }
}
