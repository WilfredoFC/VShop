namespace VShop.Application.Dtos.Pago
{
    public class AzulSolicitudResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Url { get; set; } // URL del formulario de pago
        public string ErrorMessage { get; set; }
    }
}
