using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VShop.Application.Dtos.CarritoItem;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class CarritoItemService : BaseServices<CarritoItem, CarritoItemDto>, ICarritoItemService
    {
        private readonly ICarritoItemRepository _carritoItemRepository;
        private readonly IMapper _mapper;

        public CarritoItemService(IMapper mapper, ICarritoItemRepository _baseRepository) : base(mapper, _baseRepository)
        {
            _carritoItemRepository = _baseRepository;
            _mapper = mapper;
        }

        public async Task<List<CarritoItemDto>> GetDtoByUserId(string usuarioId)
        {
            var carritoItems = await _carritoItemRepository
                .GetAllQueryWithInclude(new List<string> { "Producto" })
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            return _mapper.Map<List<CarritoItemDto>>(carritoItems);
        }
    }
}
