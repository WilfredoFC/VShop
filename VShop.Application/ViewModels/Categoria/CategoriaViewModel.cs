namespace VShop.Application.ViewModels.Categoria
{
    public class CategoriaViewModel
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool EsActivo { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
