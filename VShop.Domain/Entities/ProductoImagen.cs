using VShop.Domain.Base;

namespace VShop.Domain.Entities
{
    public class ProductoImagen : BaseEntity
    {
        public int ProductoId { get; set; }
        public string UrlImagen { get; set; }
        public bool EsPrincipal { get; set; }
        public int Orden { get; set; }

        // Propiedad de navegación
        public virtual Producto Producto { get; set; }
    }
}
