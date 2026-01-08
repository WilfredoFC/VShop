using VShop.Application.ViewModels.ProductoImagen;

namespace VShop.Application.ViewModels.Producto
{
    public class ProductoDetailsViewModel
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public decimal? PrecioDescuento { get; set; }
        public decimal PrecioFinal { get; set; }
        public bool TieneDescuento { get; set; }
        public string? SKU { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public int CategoriaId { get; set; }
        public string? Categoria { get; set; }
        public int? MarcaId { get; set; }
        public string? Marca { get; set; }
        public bool EsActivo { get; set; }
        public string? EstadoStock { get; set; }
        public string? EstadoStockClase { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public List<ProductoImagenViewModel>? Imagenes { get; set; }
    }
}
