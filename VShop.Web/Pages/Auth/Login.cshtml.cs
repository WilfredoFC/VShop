using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Services;

namespace VShop.Web.Pages.Auth;

public class LoginModel(VShopApiClient api) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? Error { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Client/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var resp = await api.LoginAsync(Input.Email, Input.Password);
        if (resp == null || resp.HasError || string.IsNullOrEmpty(resp.Token))
        {
            Error = resp?.Errors?.FirstOrDefault() ?? "Credenciales inválidas.";
            return Page();
        }

        // Guardar JWT en cookie HttpOnly
        Response.Cookies.Append("access_token", resp.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        // Crear sesión de cookie con los claims del JWT
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, resp.UserId!),
            new(ClaimTypes.Email, resp.Email!),
            new(ClaimTypes.Name, $"{resp.FirstName} {resp.LastName}"),
            new("firstName", resp.FirstName!),
            new("lastName", resp.LastName!)
        };
        if (resp.Roles != null)
            claims.AddRange(resp.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return resp.Roles?.Contains("Administrador") == true
            ? RedirectToPage("/Admin/Pedidos/Index")
            : RedirectToPage("/Client/Index");
    }
}
