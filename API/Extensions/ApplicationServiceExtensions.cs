using Application.Announcements;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using Application.InfoRecaps.StatusUpdateAfter1Day;
using Application.Interface;
using Infrastructure.Security;

namespace API.Extensions
{
    // Ketika kita membuat metode Ekstensi, maka kita perlu memastikan class kita adalah static
    public static class ApplicationServiceExtensions
    {
        // Dan kemudian kita membuat metode  ekstensi itu sendiri, dan itu akan disebut public
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        /* Sekarang parameter pertama dari metode ekstensi ini adalah hal yang akan kita perluas daftar*/
        {
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Emboiii J.A.", Version = "v1" });

                    // Define BearerAuth scheme
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer"
                    });

                    // Add BearerAuth as requirement for operations
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
                });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseNpgsql(config.GetConnectionString("KoneksiKePostgreSQL"));
            });

            // Menambahkan CORS Policy
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:5173");
                });
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly));
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Create>();
            services.AddHostedService<StatusUpdateService>();
            services.AddHttpContextAccessor();
            services.AddScoped<IUserAccessor, UserAccessor>();

            return services;
        }
    }
}