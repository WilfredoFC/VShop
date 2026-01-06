using VShop.Application.Dtos.Producto;

namespace VShop.Application.Dtos.CarritoItem
{
    public class CarritoItemDto
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; } = 1;
        public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ProductoDto Producto { get; set; }
    }
}