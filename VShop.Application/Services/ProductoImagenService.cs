using AutoMapper;
using VShop.Application.Dtos.ProductoImagen;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class ProductoImagenService : BaseServices<ProductoImagen, ProductoImagenDto>, IProductoImagenService
    {
        private readonly IProductoImagenRepository _repository;
        private readonly IMapper _mapper;

        public ProductoImagenService(IMapper mapper, IProductoImagenRepository _baseRepository) : base(mapper, _baseRepository)
        {
            _repository = _baseRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductoImagenDto>> GetByProductoIdAsync(int productoId)
        {
            var imagenesQuery = await _repository.GetAllListWithInclude([]);
            var imagenes = imagenesQuery.Where(pi => pi.ProductoId == productoId);
            return _mapper.Map<IEnumerable<ProductoImagenDto>>(imagenes);
        }
    }
}
