using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Client;

public class DetailsModel(VShopApiClient api, IConfiguration config) : PageModel
{
    public ProductoDetalle? Producto { get; set; }
    public List<ProductoItem> Relacionados { get; set; } = [];
    public string ApiBase { get; set; } = "";

    public async Task OnGetAsync(int id)
    {
        ApiBase = config["ApiBaseUrl"] ?? "";
        var tProducto = api.GetProductoAsync(id);
        var tRelacionados = api.GetRelacionadosAsync(id);
        await Task.WhenAll(tProducto, tRelacionados);
        Producto = await tProducto;
        Relacionados = (await tRelacionados) ?? [];
    }
}
