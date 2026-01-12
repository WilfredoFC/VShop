using VShop.Application.Dtos.Producto;
using VShop.Domain.Entities;

namespace VShop.Application.Interfaces
{
    public interface IProductoService : IBaseServices<Producto, ProductoDto>
    {
        Task<bool> CambiarEstadoProductoAsync(int productoId);
        Task DesactivarProductosSinStockAsync();
    }
}