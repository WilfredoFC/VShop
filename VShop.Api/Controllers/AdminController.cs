using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.Application.Interfaces;

namespace VShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class AdminController(
    IAccountService accountService,
    IProductoService productoService,
    IPedidoService pedidoService) : ControllerBase
{
    // GET api/admin/usuarios
    [HttpGet("usuarios")]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await accountService.GetAllUser();
        return Ok(usuarios.Select(u => new
        {
            u.Id, u.Email, u.UserName,
            FirstName = u.Name, u.LastName, u.Cedula,
            IsActive = u.Status, Role = u.Role
        }));
    }

    // GET api/admin/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var productos = await productoService.GetAllListDto();
        var pedidos = await pedidoService.GetAllListDto();

        return Ok(new
        {
            TotalProductos = productos.Count,
            ProductosActivos = productos.Count(p => p.EsActivo),
            ProductosSinStock = productos.Count(p => p.Stock <= 0),
            TotalPedidos = pedidos.Count,
            PedidosPendientes = pedidos.Count(p => p.Estado == "Pendiente"),
            PedidosEntregados = pedidos.Count(p => p.Estado == "Entregado"),
            IngresosTotales = pedidos.Where(p => p.Estado == "Entregado").Sum(p => p.Total)
        });
    }
}
