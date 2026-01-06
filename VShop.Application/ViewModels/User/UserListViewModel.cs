namespace VShop.Application.ViewModels.User
{
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();

        // Paginación
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Filtro por rol
        public string? RoleFilter { get; set; }

        // Utilidad para saber si se puede ir a páginas anterior/siguiente
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
                
        public List<string> RoleOptions => new List<string> { "Administrador", "Cliente" };
    }
}
