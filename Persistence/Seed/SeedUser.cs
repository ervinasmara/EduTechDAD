using Domain.User;
using Microsoft.AspNetCore.Identity;

namespace Persistence.Seed;
public class SeedUser
{
    public static async Task SeedData(DataContext context, UserManager<AppUser> userManager)
    {
        if (!userManager.Users.Any())
        {
            var adminUser = new AppUser { UserName = "Zeladaa", Role = 1 };

            var result = await userManager.CreateAsync(adminUser, "Pa$$w0rd");

            if (result.Succeeded)
            {
                var admin = new Admin
                {
                    Id = Guid.NewGuid(),
                    NameAdmin = "Ekik",
                    AppUserId = adminUser.Id,
                    User = adminUser
                };

                context.Admins.Add(admin);
            }
        }

            await context.SaveChangesAsync();
    }
}