using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Admin.Marcas;

[Authorize(Roles = "Administrador")]
public class IndexModel(VShopApiClient api) : PageModel
{
    public List<MarcaItem> Marcas { get; set; } = [];

    public async Task OnGetAsync() =>
        Marcas = (await api.GetMarcasAllAsync()) ?? [];

    public async Task<IActionResult> OnPostCreateAsync(string nombre)
    {
        await api.CreateMarcaAsync(new { Nombre = nombre });
        TempData["Success"] = "Marca creada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string nombre)
    {
        await api.UpdateMarcaAsync(id, new { Nombre = nombre });
        TempData["Success"] = "Marca actualizada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await api.DeleteMarcaAsync(id);
        TempData["Success"] = "Marca eliminada.";
        return RedirectToPage();
    }
}
