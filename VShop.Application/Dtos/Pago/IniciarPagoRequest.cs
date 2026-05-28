namespace VShop.Application.Dtos.Pago
{
    public class IniciarPagoRequest
    {
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Telefono { get; set; }
        public string Notas { get; set; }
    }
}
