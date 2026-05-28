using VShop.Application.Dtos.PedidoDetalle;

namespace VShop.Application.Dtos.Pedido
{
    public class PedidoDto : BaseDto
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

        // ========== DESHABILITADO: Campos Azul (Pagos desactivados) ==========
        // public string AzulToken { get; set; }
        // public string AzulAuthorizationCode { get; set; }
        // public string AzulResponseCode { get; set; }
        // public string AzulIsoCode { get; set; }
        // public string AzulErrorDescripcion { get; set; }
        // public DateTime? FechaPago { get; set; }
        // ====================================================================

        // Propiedades de navegación
        public virtual ICollection<PedidoDetalleDto> Detalles { get; set; }
    }
}