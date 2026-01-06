using VShop.Application.Dtos.Producto;

namespace VShop.Application.Dtos.Categoria
{
    public class CategoriaDto : BaseDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; } // "Personal" o "Hogar"

        public virtual ICollection<ProductoDto> Productos { get; set; }
    }
}
