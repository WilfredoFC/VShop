using VShop.Application.Dtos.Producto;

namespace VShop.Application.Dtos.ProductoImagen
{
    public class ProductoImagenDto : BaseDto
    {
        public int ProductoId { get; set; }
        public byte[]? Datos { get; set; }
        public string? TipoContenido { get; set; }
        public bool EsPrincipal { get; set; }
        public int Orden { get; set; }

        // Propiedad de navegación
        public virtual ProductoDto Producto { get; set; }
    }
}
