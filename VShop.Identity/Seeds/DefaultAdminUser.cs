using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VShop.Identity.Entities;

namespace VShop.Identity.Seeds
{
    public class DefaultAdminUser
    {
        public async static Task SeedAsync(UserManager<AppUser> UserManager)
        {
            AppUser user = new()
            {
                FirstName = "Administrador",
                LastName = "Principal",
                Cedula = "000001",
                Email = "Admin@gmail.com",
                Status = true,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "Admin"
            };

            if (await UserManager.Users.AllAsync(u => u.Id != user.Id))
            {

                var entityUser = await UserManager.FindByEmailAsync(user.Email);
                if (entityUser == null)
                {
                    await UserManager.CreateAsync(user, "Dharafeliz2713!");
                    await UserManager.AddToRoleAsync(user, "Administrador");
                }
            }
        }
    }
}
