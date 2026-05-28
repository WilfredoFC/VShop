namespace VShop.Application.Dtos.Pago
{
    public class IniciarPagoResponseDto
    {
        public bool Exitoso { get; set; }
        public string Token { get; set; }
        public string UrlPago { get; set; }  // URL de Azul a donde redirigir
        public string MensajeError { get; set; }
        public string NumeroOrden { get; set; } // El OrderNumber que generaste
    }
}
