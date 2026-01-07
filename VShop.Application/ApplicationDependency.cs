using Microsoft.Extensions.DependencyInjection;
using VShop.Application.Interfaces;
using VShop.Application.Services;

namespace VShop.Application
{
    public static class ApplicationDependency
    {
        public static void AddApplicationLayer(this IServiceCollection service)
        {
            service.AddAutoMapper(typeof(ApplicationDependency).Assembly);

            #region Services IOC
            service.AddTransient(typeof(IBaseServices<,>), typeof(BaseServices<,>));

            service.AddScoped<ICarritoItemService, CarritoItemService>();
            service.AddScoped<ICategoriaService, CategoriaService>();
            service.AddScoped<IInventarioMovimientoService, InventarioMovimientoService>();
            service.AddScoped<IMarcaService, MarcaService>();
            service.AddScoped<IPedidoDetalleService, PedidoDetalleService>();
            service.AddScoped<IPedidoService, PedidoService>();
            service.AddScoped<IProductoImagenService, ProductoImagenService>();
            service.AddScoped<IProductoService, ProductoService>();
            service.AddScoped<IResenaService, ResenaService>();
            #endregion
        }
    }
}
