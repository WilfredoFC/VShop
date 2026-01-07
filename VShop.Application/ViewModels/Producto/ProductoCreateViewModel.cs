using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using VShop.Application.ViewModels.Marca;

namespace VShop.Application.ViewModels.Productos
{
    public class ProductoCreateViewModel
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
        [Display(Name = "Nombre del Producto *")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio Normal *")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [Display(Name = "Precio con Descuento")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio con descuento debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        public decimal? PrecioDescuento { get; set; }

        [Display(Name = "Porcentaje de Descuento")]
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100%")]
        public int? PorcentajeDescuento { get; set; }

        [Required(ErrorMessage = "El SKU es obligatorio")]
        [StringLength(100, ErrorMessage = "El SKU no puede exceder los 100 caracteres")]
        [Display(Name = "SKU (Código único) *")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        [Display(Name = "Stock Inicial *")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "El stock mínimo es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        [Display(Name = "Stock Mínimo *")]
        public int StockMinimo { get; set; } = 10;

        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        [Display(Name = "Categoría *")]
        public int CategoriaId { get; set; }

        [Display(Name = "Marca")]
        public int? MarcaId { get; set; }

        [Display(Name = "Estado del Producto")]
        public bool EsActivo { get; set; } = true;

        [Display(Name = "Destacado")]
        public bool EsDestacado { get; set; }

        [Display(Name = "Nuevo")]
        public bool EsNuevo { get; set; } = true;

        // Imágenes
        [Display(Name = "Imágenes del Producto")]
        public List<IFormFile> Imagenes { get; set; } = new List<IFormFile>();

        // Para dropdowns
        public List<CategoriaViewModel> Categorias { get; set; } = new List<CategoriaViewModel>();
        public List<MarcaViewModel> Marcas { get; set; } = new List<MarcaViewModel>();

        // Propiedades calculadas
        public bool TieneDescuento => PorcentajeDescuento.HasValue && PorcentajeDescuento > 0;
    }
}