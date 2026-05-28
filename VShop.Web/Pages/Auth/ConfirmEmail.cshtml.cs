using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Services;

namespace VShop.Web.Pages.Auth;

public class ConfirmEmailModel(VShopApiClient api) : PageModel
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public async Task OnGetAsync(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            Message = "Enlace de confirmación inválido.";
            return;
        }

        var result = await api.ConfirmEmailAsync(userId, token);
        if (result != null)
        {
            Success = true;
            Message = result;
        }
        else
        {
            Message = "No se pudo confirmar el correo. El enlace puede haber expirado.";
        }
    }
}
