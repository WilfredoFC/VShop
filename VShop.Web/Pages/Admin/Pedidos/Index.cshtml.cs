using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Admin.Pedidos;

[Authorize(Roles = "Administrador")]
public class IndexModel(VShopApiClient api) : PageModel
{
    public List<PedidoResumen> Pedidos { get; set; } = [];
    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    [BindProperty(SupportsGet = true)] public string? Estado { get; set; }

    public async Task OnGetAsync() =>
        Pedidos = (await api.GetPedidosAdminAsync(Busqueda, Estado)) ?? [];
}
