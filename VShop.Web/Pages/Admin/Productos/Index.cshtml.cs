using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Admin.Productos;

[Authorize(Roles = "Administrador")]
public class IndexModel(VShopApiClient api, IConfiguration config) : PageModel
{
    public List<ProductoItem> Productos { get; set; } = [];
    public int TotalRegistros { get; set; }
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public string ApiBase { get; set; } = "";

    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;

    public async Task OnGetAsync()
    {
        ApiBase = config["ApiBaseUrl"] ?? "";
        var result = await api.GetProductosAdminAsync(Busqueda, Pagina);
        if (result != null)
        {
            Productos = result.Items ?? [];
            TotalRegistros = result.TotalRegistros;
            PaginaActual = result.PaginaActual;
            TotalPaginas = result.TotalPaginas;
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await api.DeleteProductoAsync(id);
        TempData["Success"] = "Producto eliminado.";
        return RedirectToPage();
    }
}
