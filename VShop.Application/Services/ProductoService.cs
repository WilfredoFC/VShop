using AutoMapper;
using VShop.Application.Dtos.Producto;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class ProductoService : BaseServices<Producto, ProductoDto>, IProductoService
    {
        public ProductoService(IMapper mapper, IBaseRepository<Producto> _baseRepository) : base(mapper, _baseRepository)
        {
        }
    }
}
