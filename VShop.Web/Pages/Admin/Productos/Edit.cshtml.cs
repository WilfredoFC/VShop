using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Admin.Productos;

[Authorize(Roles = "Administrador")]
public class EditModel(VShopApiClient api, IConfiguration config) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<CategoriaItem> Categorias { get; set; } = [];
    public List<MarcaItem> Marcas { get; set; } = [];
    public List<ImagenItem> Imagenes { get; set; } = [];
    public string ApiBase { get; set; } = "";
    public int ProductoId { get; set; }

    public class InputModel
    {
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string SKU { get; set; } = "";
        public decimal Precio { get; set; }
        public decimal? PrecioDescuento { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; } = 10;
        public int CategoriaId { get; set; }
        public int? MarcaId { get; set; }
        public bool EsActivo { get; set; } = true;
    }

    private async Task LoadDataAsync(int id)
    {
        ApiBase = config["ApiBaseUrl"] ?? "";
        ProductoId = id;

        var tProducto = api.GetProductoAsync(id);
        var tCat = api.GetCategoriasAllAsync();
        var tMar = api.GetMarcasAllAsync();
        await Task.WhenAll(tProducto, tCat, tMar);

        var p = await tProducto;
        if (p != null)
        {
            Input = new InputModel
            {
                Nombre = p.Nombre, Descripcion = p.Descripcion, SKU = p.SKU,
                Precio = p.Precio, PrecioDescuento = p.PrecioDescuento,
                Stock = p.Stock, CategoriaId = p.CategoriaId, MarcaId = p.MarcaId,
                EsActivo = p.EsActivo
            };
            Imagenes = p.Imagenes ?? [];
        }

        Categorias = (await tCat) ?? [];
        Marcas = (await tMar) ?? [];
    }

    public async Task OnGetAsync(int id) => await LoadDataAsync(id);

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid) { await LoadDataAsync(id); return Page(); }

        var req = new
        {
            Input.Nombre, Input.Descripcion, Input.Precio, Input.PrecioDescuento,
            Input.SKU, Input.Stock, Input.StockMinimo,
            Input.CategoriaId, Input.MarcaId, Input.EsActivo
        };

        await api.UpdateProductoAsync(id, req);
        TempData["Success"] = "Producto actualizado.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSubirImagenesAsync(int id)
    {
        var imagenes = Request.Form.Files;
        if (imagenes.Count > 0)
            await api.SubirImagenesAsync(id, (IFormFileCollection)imagenes);
        TempData["Success"] = "Imágenes subidas.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteImagenAsync(int id, int imagenId)
    {
        await api.EliminarImagenAsync(imagenId);
        TempData["Success"] = "Imagen eliminada.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSetPrincipalAsync(int id, int imagenId)
    {
        await api.SetImagenPrincipalAsync(imagenId);
        TempData["Success"] = "Imagen principal actualizada.";
        return RedirectToPage(new { id });
    }
}
