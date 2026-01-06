using System.ComponentModel.DataAnnotations;

namespace VShop.Application.ViewModels.User
{
    public class ForgotPasswordRequestViewModel
    {
        [Required(ErrorMessage = "Ingresa tu nombre de ususario")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }      
    }
}
