using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Auth;

public class RegisterModel(VShopApiClient api) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Success { get; set; }
    public List<string> Errors { get; set; } = [];

    public class InputModel
    {
        [Required] public string FirstName { get; set; } = "";
        [Required] public string LastName { get; set; } = "";
        public string? Cedula { get; set; }
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, MinLength(8)] public string Password { get; set; } = "";
        [Required, Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")] public string ConfirmPassword { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var req = new RegisterRequest(
            Input.Email, Input.Password, Input.ConfirmPassword,
            Input.FirstName, Input.LastName, Input.Cedula);

        await api.RegisterAsync(req);
        Success = true;
        return Page();
    }
}
