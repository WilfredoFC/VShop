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

            //service.AddScoped<ISavingsAccountServices, SavingsAccountServices>();
            #endregion

            











        }
    }
}
