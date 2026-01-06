using VShop.Domain.Entities;
using VShop.Domain.Interfaces;
using VShop.Persistences.Context;

namespace VShop.Persistences.Repositories
{
    public class PedidoDetalleRepository : BaseRepository<PedidoDetalle>, IPedidoDetalleRepository
    {
        public PedidoDetalleRepository(VShopContextDb context) : base(context)
        {

        }
    }
}
