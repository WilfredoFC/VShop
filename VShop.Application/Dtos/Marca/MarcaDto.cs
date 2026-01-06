using VShop.Application.Dtos.Producto;

namespace VShop.Application.Dtos.Marca
{
    public class MarcaDto : BaseDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        // Propiedades de navegación
        public virtual ICollection<ProductoDto> Productos { get; set; }
    }
}
