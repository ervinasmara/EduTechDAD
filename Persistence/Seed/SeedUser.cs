using Domain.User;
using Microsoft.AspNetCore.Identity;

namespace Persistence.Seed
{
    public class SeedUser
    {
        public static async Task SeedData(DataContext context, UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any()) // untuk memeriksa apakah kita memiliki User? Jika tidak maka akan dibuatkan
            {
                var users = new List<AppUser>
                {
                    new AppUser{UserName = "admin", Role = 1},
                    new AppUser{UserName = "guru", Role = 2},
                    new AppUser{UserName = "siswa", Role = 3}
                };

                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Pa$$w0rd");
                }
            }

            await context.SaveChangesAsync();
        }
    }
}