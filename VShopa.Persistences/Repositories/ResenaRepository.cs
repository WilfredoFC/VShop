using VShop.Domain.Entities;
using VShop.Domain.Interfaces;
using VShop.Persistences.Context;

namespace VShop.Persistences.Repositories
{
    public class ResenaRepository : BaseRepository<Resena>, IResenaRepository
    {
        public ResenaRepository(VShopContextDb context) : base(context)
        {
        }
    }
}
