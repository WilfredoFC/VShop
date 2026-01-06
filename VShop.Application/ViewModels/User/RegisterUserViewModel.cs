using System.ComponentModel.DataAnnotations;

namespace VShop.Application.ViewModels.User
{
    public class RegisterUserViewModel
    {
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [DataType(DataType.Text)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Debes de introduccir una cedula ")]
        [DataType(DataType.Text)]
        public required string Cedula { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Debes de introduccir algun nombre de usuario ")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "YDebes de ingresar unsa contraseña")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Compare(nameof(Password),ErrorMessage = "las Contraseñas no son iguales")]
        [Required(ErrorMessage = "Confirma la contraseña")]
        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; }

    }
}
