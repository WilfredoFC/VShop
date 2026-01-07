namespace VShop.Application.ViewModels.Productos
{
    public class ProductoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public decimal? PrecioDescuento { get; set; }
        public string SKU { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; }
        public string Marca { get; set; }
        public string ImagenPrincipal { get; set; }
        public bool EsActivo { get; set; }
        public bool TieneDescuento => PrecioDescuento.HasValue && PrecioDescuento < Precio;
        public decimal PrecioFinal => TieneDescuento ? PrecioDescuento.Value : Precio;
        public string EstadoStock => Stock <= 0 ? "Agotado" : Stock <= 10 ? "Bajo" : "Disponible";
        public string EstadoStockClase => Stock <= 0 ? "danger" : Stock <= 10 ? "warning" : "success";
    }
}
