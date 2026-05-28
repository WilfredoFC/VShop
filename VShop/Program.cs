using Microsoft.EntityFrameworkCore;
using VShop.Application;
using VShop.Helpers;
using VShop.Identity;
using VShop.Identity.Context;
using VShop.Persistences;
using VShop.Persistences.Context;
using VShop.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddPersistenceDependencies(builder.Configuration);
builder.Services.AddIdentityIocForWebApp(builder.Configuration);
builder.Services.AddSharedLayerIoc(builder.Configuration);
builder.Services.AddApplicationLayer();

builder.Services.AddScoped<FileManager>();

// ========== DESHABILITADO: HttpClient para Azul (Pagos desactivados) ==========
// builder.Services.AddHttpClient("AzulClient", client =>
// {
//     client.BaseAddress = new Uri("https://pagos.azul.com.do/webservices/JSON/");
// });
// ==============================================================================

var app = builder.Build();

// Apply migrations and seed data
try
{
    using (var scope = app.Services.CreateScope())
    {
        var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
        var persistenceContext = scope.ServiceProvider.GetRequiredService<VShopContextDb>();

        // Apply pending migrations
        await identityContext.Database.MigrateAsync();
        await persistenceContext.Database.MigrateAsync();
    }

    // Seed default data
    await app.Services.RunSeedAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during database initialization.");
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Client}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();