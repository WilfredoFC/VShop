namespace VShop.Domain.Entities
{
    public class CarritoItem
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; } = 1;
        public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual Producto Producto { get; set; }
    }
}