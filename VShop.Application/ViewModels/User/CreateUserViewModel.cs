using System.ComponentModel.DataAnnotations;

namespace VShop.Application.ViewModels.User
{
    public class CreateUserViewModel
    {
        public string Id { get; set; } = "";

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

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public  string? Password { get; set; }

        [Compare(nameof(Password),ErrorMessage = "las Contraseñas no son iguales")]
        [Required(ErrorMessage = "Debes confimar la contraseña")]
        [DataType(DataType.Password)]
        public  string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El usuario debe tener un rol valido")]
        public required string Role { get; set; }
        public bool Status { get; set; } 

    }
}
