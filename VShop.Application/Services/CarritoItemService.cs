using AutoMapper;
using VShop.Application.Dtos.CarritoItem;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class CarritoItemService : BaseServices<CarritoItem, CarritoItemDto>, ICarritoItemService
    {
        public CarritoItemService(IMapper mapper, ICarritoItemRepository _baseRepository) : base(mapper, _baseRepository)
        {
        }
    }
}
