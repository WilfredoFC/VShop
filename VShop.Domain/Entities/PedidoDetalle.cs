namespace VShop.Domain.Entities
{
    public class PedidoDetalle
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Propiedades de navegación
        public virtual Pedido Pedido { get; set; }
        public virtual Producto Producto { get; set; }
    }
}