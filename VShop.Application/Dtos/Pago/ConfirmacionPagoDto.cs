namespace VShop.Application.Dtos.Pago
{
    public class ConfirmacionPagoDto
    {
        public string OrderNumber { get; set; }
        public string AuthorizationCode { get; set; }
        public string ResponseCode { get; set; }
        public string IsoCode { get; set; }
        public string ErrorDescription { get; set; }
        public decimal Amount { get; set; }
        // Otros campos que Azul envíe
    }
}
