using VShop.Application.Dtos.CarritoItem;
using VShop.Application.Dtos.Categoria;
using VShop.Application.Dtos.InventarioMovimiento;
using VShop.Application.Dtos.Marca;
using VShop.Application.Dtos.PedidoDetalle;
using VShop.Application.Dtos.ProductoImagen;
using VShop.Application.Dtos.Resena;

namespace VShop.Application.Dtos.Producto
{
    public class ProductoDto : BaseDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public decimal? PrecioDescuento { get; set; }
        public string SKU { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; } = 10;

        // Claves foráneas
        public int CategoriaId { get; set; }
        public int? MarcaId { get; set; }

        // Propiedades de navegación
        public virtual CategoriaDto Categoria { get; set; }
        public virtual MarcaDto Marca { get; set; }
        public virtual ICollection<ProductoImagenDto> Imagenes { get; set; }
        public virtual ICollection<PedidoDetalleDto> PedidoDetalles { get; set; }
        public virtual ICollection<ResenaDto> Resenas { get; set; }
        public virtual ICollection<CarritoItemDto> CarritoItems { get; set; }
        public virtual ICollection<InventarioMovimientoDto> InventarioMovimientos { get; set; }

        // Propiedades de solo lectura (no se mapean a BD)
        public decimal PrecioFinal => PrecioDescuento.HasValue && PrecioDescuento < Precio ? PrecioDescuento.Value : Precio;
        public bool TieneDescuento => PrecioDescuento.HasValue && PrecioDescuento < Precio;
        public bool StockBajo => Stock <= StockMinimo;
    }
}