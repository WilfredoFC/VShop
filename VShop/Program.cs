using VShop.Application;
using VShop.Helpers;
using VShop.Identity;
using VShop.Persistences;
using VShop.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddPersistenceDependencies(builder.Configuration);
builder.Services.AddIdentityIocForWebApp(builder.Configuration);
builder.Services.AddSharedLayerIoc(builder.Configuration);
builder.Services.AddApplicationLayer();

builder.Services.AddScoped<FileManager>();
builder.Services.AddAutoMapper(typeof(Program));


var app = builder.Build();

await app.Services.RunSeedAsync();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Client}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();