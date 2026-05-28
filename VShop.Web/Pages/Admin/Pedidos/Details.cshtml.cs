using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Admin.Pedidos;

[Authorize(Roles = "Administrador")]
public class DetailsModel(VShopApiClient api) : PageModel
{
    public PedidoDetalle? Pedido { get; set; }

    public async Task OnGetAsync(int id) =>
        Pedido = await api.GetPedidoAdminAsync(id);

    public async Task<IActionResult> OnPostActualizarEstadoAsync(int id, string estado)
    {
        await api.ActualizarEstadoPedidoAsync(id, estado);
        TempData["Success"] = "Estado actualizado correctamente.";
        return RedirectToPage(new { id });
    }
}
