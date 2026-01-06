using VShop.Domain.Base;

namespace VShop.Domain.Entities
{
    public class Categoria : BaseEntity
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; } // "Personal" o "Hogar"

        public virtual ICollection<Producto> Productos { get; set; }
    }
}
