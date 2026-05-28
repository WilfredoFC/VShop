using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VShop.Web.Models;
using VShop.Web.Services;

namespace VShop.Web.Pages.Carrito;

[Authorize]
public class IndexModel(VShopApiClient api, IConfiguration config) : PageModel
{
    public CarritoResponse? Carrito { get; set; }
    public string ApiBase { get; set; } = "";

    [BindProperty]
    public CheckoutInput Checkout { get; set; } = new();

    public string? Error { get; set; }
    public bool PedidoCreado { get; set; }

    public class CheckoutInput
    {
        [Required(ErrorMessage = "La dirección es requerida.")]
        public string DireccionEnvio { get; set; } = "";

        [Required(ErrorMessage = "La ciudad es requerida.")]
        public string Ciudad { get; set; } = "";

        [Required(ErrorMessage = "El teléfono es requerido.")]
        public string Telefono { get; set; } = "";

        public string MetodoPago { get; set; } = "Tarjeta";
        public string? Notas { get; set; }
    }

    public async Task OnGetAsync()
    {
        ApiBase = config["ApiBaseUrl"] ?? "";
        Carrito = await api.GetCarritoAsync();
    }

    public async Task<IActionResult> OnPostActualizarAsync(int itemId, int cantidad)
    {
        if (cantidad < 1)
            await api.EliminarDelCarritoAsync(itemId);
        else
            await api.ActualizarCantidadAsync(itemId, cantidad);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int itemId)
    {
        await api.EliminarDelCarritoAsync(itemId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostVaciarAsync()
    {
        await api.VaciarCarritoAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckoutAsync()
    {
        if (!ModelState.IsValid)
        {
            ApiBase = config["ApiBaseUrl"] ?? "";
            Carrito = await api.GetCarritoAsync();
            return Page();
        }

        var result = await api.CrearPedidoAsync(new
        {
            DireccionEnvio = Checkout.DireccionEnvio,
            Ciudad = Checkout.Ciudad,
            TelefonoContacto = Checkout.Telefono,
            MetodoPago = Checkout.MetodoPago,
            Notas = Checkout.Notas
        });

        if (result == null)
        {
            Error = "No se pudo crear el pedido. Verifica que el carrito no esté vacío.";
            ApiBase = config["ApiBaseUrl"] ?? "";
            Carrito = await api.GetCarritoAsync();
            return Page();
        }

        PedidoCreado = true;
        return Page();
    }
}
