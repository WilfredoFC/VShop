using System.ComponentModel.DataAnnotations;

namespace VShop.Application.ViewModels.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
