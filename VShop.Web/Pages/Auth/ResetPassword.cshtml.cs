using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Auth;

public class ResetPasswordModel(VShopApiClient api) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = "";

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Success { get; set; }
    public List<string> Errors { get; set; } = [];

    public class InputModel
    {
        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(8, ErrorMessage = "Mínimo 8 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Debes confirmar la contraseña.")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
    }

    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Token))
            return RedirectToPage("/Auth/Login");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await api.ResetPasswordAsync(new ResetPasswordRequest(UserId, Token, Input.Password));

        if (result == null)
        {
            Errors = ["No se pudo restablecer la contraseña. El enlace puede haber expirado."];
            return Page();
        }

        Success = true;
        return Page();
    }
}
