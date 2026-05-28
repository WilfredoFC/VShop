using VShop.Domain.Entities;
using VShop.Domain.Interfaces;
using VShop.Persistences.Context;

namespace VShop.Persistences.Repositories
{
    public class MarcaRepository : BaseRepository<Marca>, IMarcaRepository
    {
        public MarcaRepository(VShopContextDb context) : base(context)
        {

        }
    }
}
