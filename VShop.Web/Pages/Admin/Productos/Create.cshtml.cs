using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Admin.Productos;

[Authorize(Roles = "Administrador")]
public class CreateModel(VShopApiClient api) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<CategoriaItem> Categorias { get; set; } = [];
    public List<MarcaItem> Marcas { get; set; } = [];

    public class InputModel
    {
        [Required] public string Nombre { get; set; } = "";
        [Required] public string Descripcion { get; set; } = "";
        [Required] public string SKU { get; set; } = "";
        [Required, Range(0, double.MaxValue)] public decimal Precio { get; set; }
        public decimal? PrecioDescuento { get; set; }
        [Required, Range(0, int.MaxValue)] public int Stock { get; set; }
        public int StockMinimo { get; set; } = 10;
        [Required] public int CategoriaId { get; set; }
        public int? MarcaId { get; set; }
        public bool EsActivo { get; set; } = true;
    }

    public async Task OnGetAsync()
    {
        var tCat = api.GetCategoriasAllAsync();
        var tMar = api.GetMarcasAllAsync();
        await Task.WhenAll(tCat, tMar);
        Categorias = (await tCat) ?? [];
        Marcas = (await tMar) ?? [];
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var req = new
        {
            Input.Nombre, Input.Descripcion, Input.Precio, Input.PrecioDescuento,
            Input.SKU, Input.Stock, Input.StockMinimo,
            Input.CategoriaId, Input.MarcaId, Input.EsActivo
        };

        var saved = await api.CreateProductoAsync(req);
        if (saved == null)
        {
            TempData["Error"] = "Error al guardar el producto.";
            await OnGetAsync();
            return Page();
        }

        // Subir imágenes si las hay
        var imagenes = Request.Form.Files;
        if (imagenes.Count > 0 && saved.Id > 0)
            await api.SubirImagenesAsync(saved.Id, (IFormFileCollection)imagenes);

        TempData["Success"] = "Producto creado correctamente.";
        return RedirectToPage("/Admin/Productos/Index");
    }
}
