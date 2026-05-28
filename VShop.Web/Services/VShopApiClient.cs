using System.Net.Http.Json;
using VShop.Web.Models;

namespace VShop.Web.Services;

public class VShopApiClient(HttpClient http)
{
    // ── Auth ──────────────────────────────────────────────────────────────────

    public Task<AuthResponse?> LoginAsync(string email, string password) =>
        PostAsync<AuthResponse>("api/auth/login", new { Email = email, Password = password });

    public Task<object?> RegisterAsync(RegisterRequest req) =>
        PostAsync<object>("api/auth/register", req);

    public Task<object?> ForgotPasswordAsync(string email) =>
        PostAsync<object>("api/auth/forgot-password", new { Email = email });

    public Task<object?> ResetPasswordAsync(ResetPasswordRequest req) =>
        PostAsync<object>("api/auth/reset-password", req);

    public Task<string?> ConfirmEmailAsync(string userId, string token) =>
        GetAsync<string>($"api/auth/confirm-email?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}");

    // ── Productos (público) ───────────────────────────────────────────────────

    public Task<PaginatedResult<ProductoItem>?> GetProductosAsync(
        string? busqueda = null, int? categoriaId = null, int? marcaId = null,
        bool soloConStock = true, bool soloConDescuento = false,
        string ordenar = "nombre", int pagina = 1, int por = 12)
    {
        var qs = BuildQs(
            ("busqueda", busqueda), ("categoriaId", categoriaId?.ToString()),
            ("marcaId", marcaId?.ToString()), ("soloConStock", soloConStock.ToString().ToLower()),
            ("soloConDescuento", soloConDescuento.ToString().ToLower()),
            ("ordenar", ordenar), ("pagina", pagina.ToString()), ("por", por.ToString()));
        return GetAsync<PaginatedResult<ProductoItem>>($"api/productos{qs}");
    }

    public Task<ProductoDetalle?> GetProductoAsync(int id) =>
        GetAsync<ProductoDetalle>($"api/productos/{id}");

    public Task<List<ProductoItem>?> GetRelacionadosAsync(int id) =>
        GetAsync<List<ProductoItem>>($"api/productos/{id}/relacionados");

    public Task<List<ProductoItem>?> BuscarSugerenciasAsync(string q) =>
        GetAsync<List<ProductoItem>>($"api/productos/buscar?q={Uri.EscapeDataString(q)}");

    // ── Productos (admin) ─────────────────────────────────────────────────────

    public Task<PaginatedResult<ProductoItem>?> GetProductosAdminAsync(
        string? busqueda = null, int pagina = 1, int por = 20)
    {
        var qs = BuildQs(("busqueda", busqueda), ("pagina", pagina.ToString()), ("por", por.ToString()));
        return GetAsync<PaginatedResult<ProductoItem>>($"api/productos/admin{qs}");
    }

    public Task<ProductoItem?> CreateProductoAsync(object req) =>
        PostAsync<ProductoItem>("api/productos", req);

    public Task<ProductoItem?> UpdateProductoAsync(int id, object req) =>
        PutAsync<ProductoItem>($"api/productos/{id}", req);

    public Task DeleteProductoAsync(int id) =>
        DeleteAsync($"api/productos/{id}");

    public async Task SubirImagenesAsync(int id, IFormFileCollection imagenes)
    {
        using var form = new MultipartFormDataContent();
        foreach (var img in imagenes)
        {
            var stream = img.OpenReadStream();
            form.Add(new StreamContent(stream), "imagenes", img.FileName);
        }
        await http.PostAsync($"api/productos/{id}/imagenes", form);
    }

    public Task EliminarImagenAsync(int imagenId) =>
        DeleteAsync($"api/productos/imagenes/{imagenId}");

    public Task SetImagenPrincipalAsync(int imagenId) =>
        PatchAsync($"api/productos/imagenes/{imagenId}/principal");

    // ── Categorías ────────────────────────────────────────────────────────────

    public Task<List<CategoriaItem>?> GetCategoriasAsync() =>
        GetAsync<List<CategoriaItem>>("api/categorias");

    public Task<List<CategoriaItem>?> GetCategoriasAllAsync() =>
        GetAsync<List<CategoriaItem>>("api/categorias/all");

    public Task<CategoriaItem?> CreateCategoriaAsync(object req) =>
        PostAsync<CategoriaItem>("api/categorias", req);

    public Task<CategoriaItem?> UpdateCategoriaAsync(int id, object req) =>
        PutAsync<CategoriaItem>($"api/categorias/{id}", req);

    public Task DeleteCategoriaAsync(int id) =>
        DeleteAsync($"api/categorias/{id}");

    // ── Marcas ────────────────────────────────────────────────────────────────

    public Task<List<MarcaItem>?> GetMarcasAsync() =>
        GetAsync<List<MarcaItem>>("api/marcas");

    public Task<List<MarcaItem>?> GetMarcasAllAsync() =>
        GetAsync<List<MarcaItem>>("api/marcas/all");

    public Task<MarcaItem?> CreateMarcaAsync(object req) =>
        PostAsync<MarcaItem>("api/marcas", req);

    public Task<MarcaItem?> UpdateMarcaAsync(int id, object req) =>
        PutAsync<MarcaItem>($"api/marcas/{id}", req);

    public Task DeleteMarcaAsync(int id) =>
        DeleteAsync($"api/marcas/{id}");

    // ── Carrito ───────────────────────────────────────────────────────────────

    public Task<CarritoResponse?> GetCarritoAsync() =>
        GetAsync<CarritoResponse>("api/carrito");

    public Task<object?> AgregarAlCarritoAsync(int productoId, int cantidad) =>
        PostAsync<object>("api/carrito", new { ProductoId = productoId, Cantidad = cantidad });

    public Task<object?> ActualizarCantidadAsync(int itemId, int nuevaCantidad) =>
        PutAsync<object>($"api/carrito/{itemId}", new { NuevaCantidad = nuevaCantidad });

    public Task EliminarDelCarritoAsync(int itemId) =>
        DeleteAsync($"api/carrito/{itemId}");

    public Task VaciarCarritoAsync() =>
        DeleteAsync("api/carrito");

    // ── Pedidos ───────────────────────────────────────────────────────────────

    public Task<List<PedidoResumen>?> GetMisPedidosAsync() =>
        GetAsync<List<PedidoResumen>>("api/pedidos");

    public Task<PedidoDetalle?> GetPedidoAsync(int id) =>
        GetAsync<PedidoDetalle>($"api/pedidos/{id}");

    public Task<object?> CrearPedidoAsync(object req) =>
        PostAsync<object>("api/pedidos", req);

    // ── Admin ─────────────────────────────────────────────────────────────────

    public Task<DashboardStats?> GetDashboardAsync() =>
        GetAsync<DashboardStats>("api/admin/dashboard");

    public Task<List<UsuarioItem>?> GetUsuariosAsync() =>
        GetAsync<List<UsuarioItem>>("api/admin/usuarios");

    public Task<List<PedidoResumen>?> GetPedidosAdminAsync(string? busqueda = null, string? estado = null)
    {
        var qs = BuildQs(("busqueda", busqueda), ("estado", estado));
        return GetAsync<List<PedidoResumen>>($"api/pedidos/admin{qs}");
    }

    public Task<PedidoDetalle?> GetPedidoAdminAsync(int id) =>
        GetAsync<PedidoDetalle>($"api/pedidos/admin/{id}");

    public Task<object?> ActualizarEstadoPedidoAsync(int id, string estado) =>
        PutAsync<object>($"api/pedidos/admin/{id}/estado", new { Estado = estado });

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            var resp = await http.GetAsync(url);
            return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<T>() : default;
        }
        catch { return default; }
    }

    private async Task<T?> PostAsync<T>(string url, object body)
    {
        try
        {
            var resp = await http.PostAsJsonAsync(url, body);
            return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<T>() : default;
        }
        catch { return default; }
    }

    private async Task<T?> PutAsync<T>(string url, object body)
    {
        try
        {
            var resp = await http.PutAsJsonAsync(url, body);
            return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<T>() : default;
        }
        catch { return default; }
    }

    private async Task DeleteAsync(string url)
    {
        try { await http.DeleteAsync(url); }
        catch { }
    }

    private async Task PatchAsync(string url)
    {
        try { await http.PatchAsync(url, null); }
        catch { }
    }

    private static string BuildQs(params (string Key, string? Value)[] pairs)
    {
        var parts = pairs.Where(p => !string.IsNullOrEmpty(p.Value))
                         .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}");
        var qs = string.Join("&", parts);
        return string.IsNullOrEmpty(qs) ? "" : "?" + qs;
    }
}
