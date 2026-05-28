using Microsoft.EntityFrameworkCore;
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

        public async Task DesactivarProductosSinStockAsync()
        {
            // Método batch para desactivar todos los productos sin stock
            var productosSinStock = await _context.Productos
                .Where(p => p.Stock <= 0 && p.EsActivo)
                .ToListAsync();

            foreach (var producto in productosSinStock)
            {
                producto.EsActivo = producto.EsActivo;
                producto.Stock = 0;
                producto.FechaActualizacion = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
