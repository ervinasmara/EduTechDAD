using API.Services;
using Domain.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using System.Text;

namespace API.Extensions;
public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services,
        IConfiguration config)
    {
        /* AddIdentityCore hanya menambah dan mengkonfigurasi sistem identitas untuk tipe pengguna yang ditentukan */
        services.AddIdentityCore<AppUser>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
        })
            .AddEntityFrameworkStores<DataContext>();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])); // Password ini samakan dengan password yang ada di "TokenService.cs"

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        services.AddScoped<TokenService>();

        return services;
    }
}