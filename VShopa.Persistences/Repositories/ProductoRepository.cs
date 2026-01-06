using VShop.Domain.Entities;
using VShop.Domain.Interfaces;
using VShop.Persistences.Context;

namespace VShop.Persistences.Repositories
{
    public class ProductoRepository : BaseRepository<Producto>, IProductoRepository
    {
        private readonly VShopContextDb _context;
        public ProductoRepository(VShopContextDb context) : base(context)
        {
            _context = context;
        }
    }
}
