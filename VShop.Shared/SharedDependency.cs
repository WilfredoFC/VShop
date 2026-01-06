using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VShop.Application.Interfaces;
using VShop.Domain.Setting;
using VShop.Shared.Services;

namespace VShop.Shared
{
    public static class SharedDependency
    {
        public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();
            #endregion
        }
    }
}
