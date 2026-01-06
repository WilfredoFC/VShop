using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VShop.Identity.Entities;

namespace VShop.Identity.Seeds
{
    public class DefaultClientUser
    {
        public async static Task SeedAsync(UserManager<AppUser> userManager)
        {
            AppUser user = new()
            {
                FirstName = "Wilfredo Valentin",
                LastName = "Feliz Caba",
                Cedula = "402-0873439-8",
                Email = "wfelizcaba7@gmail.com",
                EmailConfirmed = true,
                Status = true,
                PhoneNumberConfirmed = true,
                UserName = "Wil"
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var existingUser = await userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    var result = await userManager.CreateAsync(user, "Dharafeliz2713!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Cliente");
                    }
                }
            }
        }
    }
}
