using VShop.Domain.Entities;
using VShop.Domain.Interfaces;
using VShop.Persistences.Context;

namespace VShop.Persistences.Repositories
{
    public class CarritoItemRepository : BaseRepository<CarritoItem>, ICarritoItemRepository
    {
        public CarritoItemRepository(VShopContextDb context) : base(context)
        {
        }
    }
}
