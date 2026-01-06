using VShop.Domain.Base;

namespace VShop.Domain.Entities
{
    public class Marca : BaseEntity
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        // Propiedades de navegación
        public virtual ICollection<Producto> Productos { get; set; }
    }
}
