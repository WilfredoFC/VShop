namespace VShop.Application.ViewModels.ProductoImagen
{
    public class ProductoImagenViewModel
    {
        public int Id { get; set; }
        public string? UrlImagen { get; set; }
        public bool EsPrincipal { get; set; }
        public int Orden { get; set; }
        public bool EsActivo { get; set; }
    }
}
