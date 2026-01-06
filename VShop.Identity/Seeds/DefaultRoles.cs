using Microsoft.AspNetCore.Identity;

namespace VShop.Identity.Seeds
{
    public static class DefaulRoles
    {
        public async static Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole("Administrador"));
            await roleManager.CreateAsync(new IdentityRole("Cliente"));

        }
    }
}
