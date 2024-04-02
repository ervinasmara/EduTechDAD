using API.Extensions;
using API.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Seed;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(opt =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddIdentityService(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireRole1", policy =>
        policy.RequireClaim(ClaimTypes.Role, "1"));
    options.AddPolicy("RequireRole2", policy =>
        policy.RequireClaim(ClaimTypes.Role, "2"));
    options.AddPolicy("RequireRole3", policy =>
        policy.RequireClaim(ClaimTypes.Role, "3"));
    options.AddPolicy("RequireRole4", policy =>
        policy.RequireClaim(ClaimTypes.Role, "4"));

    // Penggabungan role 1 & 4
    options.AddPolicy("RequireRole1OrRole4", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "1") ||
        context.User.HasClaim(ClaimTypes.Role, "4")));

    // Penggabungan role 2 & 4
    options.AddPolicy("RequireRole2OrRole4", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "2") ||
        context.User.HasClaim(ClaimTypes.Role, "4")));

    // Penggabungan role 3 & 4
    options.AddPolicy("RequireRole3OrRole4", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "3") ||
        context.User.HasClaim(ClaimTypes.Role, "4")));

    // Penggabungan role 1, 2, & 4
    options.AddPolicy("RequireRole1,2,4", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "1") ||
        context.User.HasClaim(ClaimTypes.Role, "2") ||
        context.User.HasClaim(ClaimTypes.Role, "4")));

    // Penggabungan role 2, 3, & 4
    options.AddPolicy("RequireRole2,3,4", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "2") ||
        context.User.HasClaim(ClaimTypes.Role, "3") ||
        context.User.HasClaim(ClaimTypes.Role, "4")));

    // Penggabungan role 1, 2, 3 & 4
    options.AddPolicy("RequireRole1,2,3,4", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "1") ||
        context.User.HasClaim(ClaimTypes.Role, "2") ||
        context.User.HasClaim(ClaimTypes.Role, "3") ||
        context.User.HasClaim(ClaimTypes.Role, "4")));
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    //var userManager = services.GetRequiredService<UserManager<AppUser>>();

    await context.Database.MigrateAsync();

    await SeedAnnouncement.SeedData(context);
    await SeedClassRoom.SeedData(context);
    await SeedLesson.SeedData(context);
    //await SeedUser.SeedData(context, userManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Terjadi kesalahan selama migrasi");
}

app.Run();