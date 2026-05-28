using Microsoft.Extensions.DependencyInjection;
using VShop.Application.Interfaces;
using VShop.Application.Services;

namespace VShop.Application
{
    public static class ApplicationDependency
    {
        public static void AddApplicationLayer(this IServiceCollection service)
        {
            service.AddAutoMapper(cfg => { }, typeof(ApplicationDependency).Assembly);

            #region Services IOC
            service.AddTransient(typeof(IBaseServices<,>), typeof(BaseServices<,>));

            // DESHABILITADO: Servicio de Azul - Pagos desactivados
            // service.AddScoped<IAzulService, AzulService>();
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
