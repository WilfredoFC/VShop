using VShop.Application.Dtos.Pedido;
using VShop.Application.Dtos.Producto;

namespace VShop.Application.Dtos.PedidoDetalle
{
    public class PedidoDetalleDto
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Propiedades de navegación
        public virtual PedidoDto Pedido { get; set; }
        public virtual ProductoDto Producto { get; set; }
    }
}