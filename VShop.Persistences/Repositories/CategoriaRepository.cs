using VShop.Domain.Entities;
using VShop.Domain.Interfaces;
using VShop.Persistences.Context;

namespace VShop.Persistences.Repositories
{
    public class CategoriaRepository : BaseRepository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(VShopContextDb context) : base(context)
        {
        }
    }
}
