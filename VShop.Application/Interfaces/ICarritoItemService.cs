using VShop.Application.Dtos.CarritoItem;
using VShop.Domain.Entities;

namespace VShop.Application.Interfaces
{
    public interface ICarritoItemService : IBaseServices<CarritoItem, CarritoItemDto>
    {
    }
}