using VShop.Application.Dtos.ProductoImagen;
using VShop.Domain.Entities;

namespace VShop.Application.Interfaces
{
    public interface IProductoImagenService : IBaseServices<ProductoImagen, ProductoImagenDto>
    {
        Task<IEnumerable<ProductoImagenDto>> GetByProductoIdAsync(int productoId);
    }
}