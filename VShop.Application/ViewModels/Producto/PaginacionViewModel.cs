namespace VShop.Application.ViewModels.Productos
{
    public class PaginacionViewModel
    {
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public int RegistrosPorPagina { get; set; } = 10;
    }
}
