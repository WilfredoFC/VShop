// CategoriaCreateViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace VShop.Application.ViewModels.Categoria
{
    public class CategoriaCreateViewModel
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre de la Categoría *")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el tipo de categoría")]
        [Display(Name = "Tipo de Categoría *")]
        public string Tipo { get; set; } = "Personal"; // Personal o Hogar

        [Display(Name = "Categoría Padre")]
        public int? CategoriaPadreId { get; set; }

        [Display(Name = "Mostrar en menú principal")]
        public bool MostrarEnMenu { get; set; } = true;

        [Display(Name = "Estado")]
        public bool EsActivo { get; set; } = true;

        [Display(Name = "Orden de visualización")]
        [Range(0, 100, ErrorMessage = "El orden debe estar entre 0 y 100")]
        public int Orden { get; set; } = 0;

        // Para dropdowns
        public List<CategoriaViewModel> CategoriasPadre { get; set; } = new List<CategoriaViewModel>();
    }
}