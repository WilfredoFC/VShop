using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Admin.Categorias;

[Authorize(Roles = "Administrador")]
public class IndexModel(VShopApiClient api) : PageModel
{
    public List<CategoriaItem> Categorias { get; set; } = [];

    public async Task OnGetAsync() =>
        Categorias = (await api.GetCategoriasAllAsync()) ?? [];

    public async Task<IActionResult> OnPostCreateAsync(string nombre, string descripcion, string tipo)
    {
        await api.CreateCategoriaAsync(new { Nombre = nombre, Descripcion = descripcion, Tipo = tipo });
        TempData["Success"] = "Categoría creada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string nombre, string descripcion, string tipo)
    {
        await api.UpdateCategoriaAsync(id, new { Nombre = nombre, Descripcion = descripcion, Tipo = tipo });
        TempData["Success"] = "Categoría actualizada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await api.DeleteCategoriaAsync(id);
        TempData["Success"] = "Categoría eliminada.";
        return RedirectToPage();
    }
}
