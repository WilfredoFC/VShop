using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VShop.Domain.Entities;

namespace VShop.Persistences.Context
{
    public class VShopContextDb : DbContext
    {
        public VShopContextDb(DbContextOptions<VShopContextDb> options) : base(options)
        {

        }

        public DbSet<CarritoItem> CarritoItems { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<InventarioMovimiento>  InventarioMovimientos { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoImagen> ProductoImagens { get; set; }
        public DbSet<Resena> Resenas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }

    }
}