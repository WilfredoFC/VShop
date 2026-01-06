using AutoMapper;
using VShop.Application.Dtos.ProductoImagen;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class ProductoImagenService : BaseServices<ProductoImagen, ProductoImagenDto>, IProductoImagenService
    {
        public ProductoImagenService(IMapper mapper, IBaseRepository<ProductoImagen> _baseRepository) : base(mapper, _baseRepository)
        {
        }
    }
}
