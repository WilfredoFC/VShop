using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using VShop.Application.ViewModels.Categoria;
using VShop.Application.ViewModels.Marca;
using VShop.Application.ViewModels.ProductoImagen;

namespace VShop.Application.ViewModels.Producto
{
    // Para el Edit
    public class ProductoEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public required string Nombre { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de descuento debe ser mayor a 0")]
        public decimal? PrecioDescuento { get; set; }

        [Required(ErrorMessage = "El SKU es obligatorio")]
        [StringLength(50)]
        public required string SKU { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int StockMinimo { get; set; } = 10;

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        public int? MarcaId { get; set; }

        public bool EsActivo { get; set; } = true;

        // Para los dropdowns
        public List<CategoriaViewModel>? Categorias { get; set; }
        public List<MarcaViewModel>? Marcas { get; set; }

        // Para nuevas imágenes
        [DataType(DataType.Upload)]
        public List<IFormFile>? Imagenes { get; set; }

        // Para imágenes existentes
        public List<ProductoImagenViewModel>? ImagenesExistentes { get; set; }

        // Para cambiar la imagen principal
        public int? ImagenPrincipalId { get; set; }
    }

}
