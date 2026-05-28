using Microsoft.AspNetCore.Authentication.Cookies;
using VShop.Web.Infrastructure;
using VShop.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Cookie auth (basada en claims del JWT recibido de la API)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Auth/Login";
        opt.LogoutPath = "/Auth/Logout";
        opt.AccessDeniedPath = "/Auth/AccessDenied";
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SameSite = SameSiteMode.Strict;
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

// Handler que inyecta el JWT (guardado en cookie) en cada llamada a la API
builder.Services.AddScoped<JwtCookieHandler>();

// HttpClient tipado hacia la API
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7200";
builder.Services.AddHttpClient<VShopApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBase);
}).AddHttpMessageHandler<JwtCookieHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Redirigir la raíz al catálogo
app.MapGet("/", () => Results.Redirect("/Client/Index"));

await app.RunAsync();
