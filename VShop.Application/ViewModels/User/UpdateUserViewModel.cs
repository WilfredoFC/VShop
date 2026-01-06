using System.ComponentModel.DataAnnotations;

namespace VShop.Application.ViewModels.User
{
    public class UpdateUserViewModel
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [DataType(DataType.Text)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "La cédula es requerida")]
        [DataType(DataType.Text)]
        public required string Cedula { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }

        // Contraseña solo si se desea cambiar
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El usuario debe tener un rol válido")]
        public required string Role { get; set; }

        public bool Status { get; set; }
    }
}
