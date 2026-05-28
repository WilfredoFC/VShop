using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Client;

public class IndexModel(VShopApiClient api, IConfiguration config) : PageModel
{
    public List<ProductoItem> Productos { get; set; } = [];
    public List<CategoriaItem> Categorias { get; set; } = [];
    public List<MarcaItem> Marcas { get; set; } = [];
    public int TotalRegistros { get; set; }
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public string ApiBase { get; set; } = "";

    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoriaId { get; set; }
    [BindProperty(SupportsGet = true)] public int? MarcaId { get; set; }
    [BindProperty(SupportsGet = true)] public bool SoloConDescuento { get; set; }
    [BindProperty(SupportsGet = true)] public string Ordenar { get; set; } = "nombre";
    [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;

    public async Task OnGetAsync()
    {
        ApiBase = config["ApiBaseUrl"] ?? "";

        var tProductos = api.GetProductosAsync(Busqueda, CategoriaId, MarcaId, true, SoloConDescuento, Ordenar, Pagina);
        var tCategorias = api.GetCategoriasAsync();
        var tMarcas = api.GetMarcasAsync();
        await Task.WhenAll(tProductos, tCategorias, tMarcas);
        var productos = await tProductos;
        var categorias = await tCategorias;
        var marcas = await tMarcas;

        if (productos != null)
        {
            Productos = productos.Items ?? [];
            TotalRegistros = productos.TotalRegistros;
            PaginaActual = productos.PaginaActual;
            TotalPaginas = productos.TotalPaginas;
        }
        Categorias = categorias ?? [];
        Marcas = marcas ?? [];
    }
}
