namespace VShop.Domain.Entities
{
    public class InventarioMovimiento
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string TipoMovimiento { get; set; } // Entrada, Salida, Ajuste, Venta, Devolucion
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public int? ReferenciaId { get; set; } // Id de pedido u otra referencia
        public string Observaciones { get; set; }
        public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
        public string UsuarioId { get; set; }

        // Propiedades de navegación
        public virtual Producto Producto { get; set; }
    }
}
