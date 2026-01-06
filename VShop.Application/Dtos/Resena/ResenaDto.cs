using VShop.Application.Dtos.Producto;

namespace VShop.Application.Dtos.Resena
{
    public class ResenaDto
    {
        public int Id { get; set; }
        public required int ProductoId { get; set; }
        public required string UsuarioId { get; set; }
        public int Calificacion { get; set; } // 1-5
        public string Comentario { get; set; }
        public DateTime FechaResena { get; set; } = DateTime.UtcNow;
        public bool EsAprobado { get; set; } = false;

        // Propiedades de navegación
        public virtual ProductoDto? Producto { get; set; }
    }
}