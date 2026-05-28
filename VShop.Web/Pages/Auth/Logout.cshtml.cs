using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VShop.Web.Pages.Auth;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        Response.Cookies.Delete("access_token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Auth/Login");
    }
}
