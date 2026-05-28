using VShop.Domain.Entities;
using VShop.Domain.Interfaces;
using VShop.Persistences.Context;

namespace VShop.Persistences.Repositories
{
    public class ProductoImagenRepository : BaseRepository<ProductoImagen>, IProductoImagenRepository
    {
        public ProductoImagenRepository(VShopContextDb context) : base(context)
        {
        }
    }
}
