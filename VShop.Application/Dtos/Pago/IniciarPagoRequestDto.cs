namespace VShop.Application.Dtos.Pago
{
    public class IniciarPagoRequestDto
    {
        public int OrdenId { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal Itbis { get; set; }
        public string Moneda { get; set; } = "DOP";
        public string? Descripcion { get; set; }
    }
}
