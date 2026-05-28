using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using VShop.Application;
using VShop.Identity;
using VShop.Identity.Context;
using VShop.Identity.Entities;
using VShop.Persistences;
using VShop.Persistences.Context;
using VShop.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// CORS — permite al frontend consumir la API
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["https://localhost:7100"];
builder.Services.AddCors(opt => opt.AddPolicy("WebPolicy", policy =>
    policy.WithOrigins(allowedOrigins)
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()));

// Capas de la aplicación
builder.Services.AddPersistenceDependencies(builder.Configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddSharedLayerIoc(builder.Configuration);

// Identity (solo UserManager / TokenProvider, sin cookie auth)
var conn = builder.Configuration.GetConnectionString("ConnectionDb");
builder.Services.AddDbContext<IdentityContext>(opt =>
    opt.UseSqlServer(conn, m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));

builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.Password.RequiredLength = 8;
    opt.Password.RequireDigit = true;
    opt.Password.RequireNonAlphanumeric = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = true;
    opt.User.RequireUniqueEmail = true;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    opt.Lockout.MaxFailedAccessAttempts = 5;
    opt.Lockout.AllowedForNewUsers = true;
});

builder.Services.AddIdentityCore<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityContext>()
    .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromHours(12));

// JWT Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

// Registrar AccountService (para register, forgot-password, confirm-email)
builder.Services.AddScoped<VShop.Application.Interfaces.IAccountService, VShop.Identity.Services.AccountService>();

var app = builder.Build();

// Migraciones + Seeds
try
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    await sp.GetRequiredService<IdentityContext>().Database.MigrateAsync();
    await sp.GetRequiredService<VShopContextDb>().Database.MigrateAsync();
    await app.Services.RunSeedAsync();
}
catch (Exception ex)
{
    app.Services.GetRequiredService<ILogger<Program>>().LogError(ex, "Error inicializando la base de datos.");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt => opt.Title = "VShop API");
}

app.UseHttpsRedirection();
app.UseCors("WebPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
