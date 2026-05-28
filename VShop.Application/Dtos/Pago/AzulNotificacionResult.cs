namespace VShop.Application.Dtos.Pago
{
    public class AzulNotificacionResult
    {
        public string OrderNumber { get; set; }
        public string AuthorizationCode { get; set; }
        public string ResponseCode { get; set; }
        public string IsoCode { get; set; }
        public string ErrorDescription { get; set; }
        public bool IsSuccessful => ResponseCode == "ISO8583" && IsoCode == "00";
    }
}
