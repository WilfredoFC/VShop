using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VShop.Domain.Interfaces;
using VShop.Persistences.Context;
using VShop.Persistences.Repositories;

namespace VShop.Persistences
{
    public static class PersistenceDependency
    {
        public static void AddPersistenceDependencies(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<VShopContextDb>(opt =>
                                              opt.UseInMemoryDatabase("Memory"));
            }
            else
            {
                var connectionString = config.GetConnectionString("ConnectionDb");
                services.AddDbContext<VShopContextDb>(opt =>
                opt.UseSqlServer(connectionString,
                m => m.MigrationsAssembly(typeof(VShopContextDb).Assembly.FullName))
                , ServiceLifetime.Scoped);
            }

            #endregion
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository< >));

            services.AddTransient<ICarritoItemRepository, CarritoItemRepository>();
            services.AddTransient<ICategoriaRepository, CategoriaRepository>();
            services.AddTransient<IInventarioMovimientoRepository, InventarioMovimientoRepository>();
            services.AddTransient<IMarcaRepository, MarcaRepository>();
            services.AddTransient<IPedidoDetalleRepository, PedidoDetalleRepository>();
            services.AddTransient<IPedidoRepository, PedidoRepository>();
            services.AddTransient<IProductoImagenRepository, ProductoImagenRepository>();
            services.AddTransient<IProductoRepository, ProductoRepository>();
            services.AddTransient<IResenaRepository, ResenaRepository>();

        }
    }
}
