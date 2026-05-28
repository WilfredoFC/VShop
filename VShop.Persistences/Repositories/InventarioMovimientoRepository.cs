using VShop.Domain.Entities;
using VShop.Domain.Interfaces;
using VShop.Persistences.Context;

namespace VShop.Persistences.Repositories
{
    public class InventarioMovimientoRepository : BaseRepository<InventarioMovimiento>, IInventarioMovimientoRepository
    {
        public InventarioMovimientoRepository(VShopContextDb context) : base(context)
        {

        }
    }
}
