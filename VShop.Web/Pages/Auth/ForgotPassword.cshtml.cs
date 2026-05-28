using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Services;

namespace VShop.Web.Pages.Auth;

public class ForgotPasswordModel(VShopApiClient api) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Sent { get; set; }
    public string? Error { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress]
        public string Email { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await api.ForgotPasswordAsync(Input.Email);
        Sent = true;
        return Page();
    }
}
