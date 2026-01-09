using VShop.Domain.Base;

namespace VShop.Domain.Entities
{
    public class Producto : BaseEntity
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public decimal? PrecioDescuento { get; set; }
        public string SKU { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; } = 10;

        // Claves foráneas
        public int? CategoriaId { get; set; }
        public int? MarcaId { get; set; }

        // Propiedades de navegación
        public virtual Categoria Categoria { get; set; }
        public virtual Marca Marca { get; set; }
        public virtual ICollection<ProductoImagen> Imagenes { get; set; }
        public virtual ICollection<PedidoDetalle> PedidoDetalles { get; set; }
        public virtual ICollection<Resena> Resenas { get; set; }
        public virtual ICollection<CarritoItem> CarritoItems { get; set; }
        public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; }

        // Propiedades de solo lectura (no se mapean a BD)
        public decimal PrecioFinal => PrecioDescuento.HasValue && PrecioDescuento < Precio ? PrecioDescuento.Value : Precio;
        public bool TieneDescuento => PrecioDescuento.HasValue && PrecioDescuento < Precio;
        public bool StockBajo => Stock <= StockMinimo;
    }
}