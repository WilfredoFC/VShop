namespace VShop.Web.Models;

// ── Auth ──────────────────────────────────────────────────────────────────────
public record LoginRequest(string Email, string Password);
public record RegisterRequest(
    string Email, string Password, string ConfirmPassword,
    string FirstName, string LastName, string? Cedula);
public record AuthResponse(
    string? Token, string? UserId, string? Email,
    string? FirstName, string? LastName, List<string>? Roles,
    bool HasError = false, List<string>? Errors = null);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string UserId, string Token, string Password);

// ── Productos ─────────────────────────────────────────────────────────────────
public record ProductoItem(
    int Id, string Nombre, string Descripcion, string SKU,
    decimal Precio, decimal? PrecioDescuento, int Stock,
    string Categoria, string Marca, int CategoriaId, int? MarcaId,
    int? ImagenPrincipalId, bool EsActivo, bool TieneDescuento, decimal PrecioFinal);

public record ProductoDetalle(
    int Id, string Nombre, string Descripcion, string SKU,
    decimal Precio, decimal? PrecioDescuento,
    int Stock, string Categoria, string Marca,
    int CategoriaId, int? MarcaId, bool EsActivo,
    bool TieneDescuento, decimal PrecioFinal,
    List<ImagenItem> Imagenes);

public record ImagenItem(int Id, bool EsPrincipal, int Orden);

public record PaginatedResult<T>(
    List<T> Items, int TotalRegistros, int PaginaActual, int TotalPaginas);

// ── Categorías / Marcas ───────────────────────────────────────────────────────
public record CategoriaItem(int Id, string Nombre, string Descripcion, string Tipo, bool EsActivo);
public record MarcaItem(int Id, string Nombre, bool EsActivo);

// ── Carrito ───────────────────────────────────────────────────────────────────
public record CarritoResponse(
    List<CarritoItemResponse> Items,
    decimal Subtotal, decimal Impuestos, decimal Total, int CantidadItems);

public record CarritoItemResponse(
    int Id, int ProductoId, string NombreProducto, int Cantidad,
    decimal PrecioUnitario, decimal Subtotal, int Stock, int? ImagenId);

// ── Pedidos ───────────────────────────────────────────────────────────────────
public record PedidoResumen(
    int Id, string NumeroPedido, string Estado,
    DateTime FechaPedido, decimal Total);

public record PedidoDetalle(
    int Id, string NumeroPedido, string UsuarioId, string Estado,
    DateTime FechaPedido, decimal Subtotal, decimal Impuestos, decimal Total,
    string MetodoPago, string DireccionEnvio, string Ciudad,
    string TelefonoContacto, string Notas,
    List<PedidoDetalleItem> Detalles);

public record PedidoDetalleItem(
    int ProductoId, string NombreProducto, int Cantidad,
    decimal PrecioUnitario, decimal Subtotal);

// ── Admin ─────────────────────────────────────────────────────────────────────
public record DashboardStats(
    int TotalProductos, int ProductosActivos, int ProductosSinStock,
    int TotalPedidos, int PedidosPendientes, int PedidosEntregados,
    decimal IngresosTotales);

public record UsuarioItem(
    string Id, string Email, string? UserName,
    string FirstName, string LastName, string? Cedula,
    bool IsActive, List<string>? Roles);
