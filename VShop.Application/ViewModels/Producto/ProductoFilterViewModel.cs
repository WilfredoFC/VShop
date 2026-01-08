using System.ComponentModel.DataAnnotations;

namespace VShop.Application.ViewModels.Producto
{
    public class ProductoFilterViewModel
    {
        [Display(Name = "Buscar")]
        public string Busqueda { get; set; }

        [Display(Name = "Categoría")]
        public int? CategoriaId { get; set; }

        [Display(Name = "Marca")]
        public int? MarcaId { get; set; }

        [Display(Name = "Estado")]
        public string EstadoStock { get; set; } // "todos", "disponible", "bajo", "agotado"

        [Display(Name = "Ordenar por")]
        public string OrdenarPor { get; set; } // "nombre", "precio_asc", "precio_desc", "stock"

        [Display(Name = "Con descuento")]
        public bool SoloConDescuento { get; set; }
    }
}
