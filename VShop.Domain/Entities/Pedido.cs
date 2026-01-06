using VShop.Domain.Base;

namespace VShop.Domain.Entities
{
    public class Pedido : BaseEntity
    {
        public string NumeroPedido { get; set; }
        public string UsuarioId { get; set; }
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmado, EnProceso, Enviado, Entregado, Cancelado
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; }
        public string DireccionEnvio { get; set; }
        public string Ciudad { get; set; }
        public string TelefonoContacto { get; set; }
        public string Notas { get; set; }

        // Propiedades de navegación
        public virtual ICollection<PedidoDetalle> Detalles { get; set; }
    }
}