using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VShop.Application.Interfaces;
using VShop.Identity.Context;
using VShop.Identity.Entities;
using VShop.Identity.Seeds;
using VShop.Identity.Services;

namespace VShop.Identity.Configurations
{
    public static class IdentityConfiguration
    {

        public static void AddIdentityIocForWebApp(this IServiceCollection services, IConfiguration config)
        {

            GeneralConfiguration(services, config);

            #region Identity

            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;

                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                opt.Lockout.MaxFailedAccessAttempts = 5;

                opt.User.RequireUniqueEmail = true;
            });

            services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole>()
                .AddSignInManager()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(12);

            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            }).AddCookie(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(180);
                opt.LoginPath = "/Login";
                opt.AccessDeniedPath = "/Login/AccessDenied";
            });
            #endregion

            services.AddScoped<IAccountService, AccountService>();
        }


        //public static void AddIdentityLayerIocForWebApi(this IServiceCollection services, IConfiguration config)
        //{
        //    GeneralConfiguration(services, config);

        //    #region Configurations
        //    services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
        //    #endregion

        //    #region Identity 
        //    services.Configure<IdentityOptions>(opt =>
        //    {
        //        opt.Password.RequiredLength = 8;
        //        opt.Password.RequireDigit = true;
        //        opt.Password.RequireNonAlphanumeric = true;
        //        opt.Password.RequireLowercase = true;
        //        opt.Password.RequireUppercase = true;

        //        opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        //        opt.Lockout.MaxFailedAccessAttempts = 5;

        //        opt.User.RequireUniqueEmail = true;
        //        opt.SignIn.RequireConfirmedEmail = true;
        //    });

        //    services.AddIdentityCore<AppUser>()
        //        .AddRoles<IdentityRole>()
        //        .AddSignInManager()
        //        .AddEntityFrameworkStores<IdentityContext>()
        //        .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

        //    services.Configure<DataProtectionTokenProviderOptions>(opt =>
        //    {
        //        opt.TokenLifespan = TimeSpan.FromHours(12);
        //    });

        //    services.AddAuthentication(opt =>
        //    {
        //        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //        opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        //    }).AddJwtBearer(opt =>
        //    {
        //        opt.RequireHttpsMetadata = false;
        //        opt.SaveToken = false;
        //        opt.TokenValidationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            ValidateIssuer = true,
        //            ValidateAudience = true,
        //            ValidateLifetime = true,
        //            ClockSkew = TimeSpan.FromMinutes(2),
        //            ValidIssuer = config["JwtSettings:Issuer"],
        //            ValidAudience = config["JwtSettings:Audience"],
        //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"] ?? ""))
        //        };
        //        opt.Events = new JwtBearerEvents()
        //        {
        //            OnAuthenticationFailed = af =>
        //            {
        //                af.NoResult();
        //                af.Response.StatusCode = 500;
        //                af.Response.ContentType = "text/plain";
        //                return af.Response.WriteAsync(af.Exception.Message.ToString());
        //            },
        //            OnChallenge = c =>
        //            {
        //                c.HandleResponse();
        //                c.Response.StatusCode = 401;
        //                c.Response.ContentType = "application/json";
        //                var result = JsonConvert.SerializeObject(new JwtResponseDto { HasError = true, Error = "You are not Authorized" });
        //                return c.Response.WriteAsync(result);
        //            },
        //            OnForbidden = c =>
        //            {
        //                c.Response.StatusCode = 403;
        //                c.Response.ContentType = "application/json";
        //                var result = JsonConvert.SerializeObject(new JwtResponseDto { HasError = true, Error = "You are not Authorized to access this resource" });
        //                return c.Response.WriteAsync(result);
        //            }
        //        };
        //    }).AddCookie("ApiCookies", opt =>
        //    {
        //        opt.ExpireTimeSpan = TimeSpan.FromMinutes(180);
        //    });
        //    #endregion

        //    #region Services
        //    services.AddScoped<IAccountServiceForWebApi, AccountServiceForWebApi>();
        //    #endregion
        //}



        public static async Task RunSeedAsync(this IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var servicesProvider = scope.ServiceProvider;

            var userManager = servicesProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = servicesProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await DefaulRoles.SeedAsync(roleManager);
            await DefaultAdminUser.SeedAsync(userManager);
            await DefaultClientUser.SeedAsync(userManager);
        }

        #region Private methods



        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<IdentityContext>(opt
                                                          => opt.UseInMemoryDatabase("AppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("ConnectionDb");
                services.AddDbContext<IdentityContext>(

                   (servicesProvider, opt) =>
                   {

                       opt.EnableSensitiveDataLogging();
                       opt.UseSqlServer(connectionString,
                       m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
                   },
                   contextLifetime: ServiceLifetime.Scoped,
                   optionsLifetime: ServiceLifetime.Scoped);
            }
            #endregion
        }
    }
}