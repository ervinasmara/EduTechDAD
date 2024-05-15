using API.Extensions;
using API.Middleware;
using Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

    // Penggabungan role 1 & 2
    options.AddPolicy("RequireRole1OrRole2", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "1") ||
        context.User.HasClaim(ClaimTypes.Role, "2")));

    // Penggabungan role 1 & 3
    options.AddPolicy("RequireRole1OrRole3", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "1") ||
        context.User.HasClaim(ClaimTypes.Role, "3")));

    // Penggabungan role 2 & 3
    options.AddPolicy("RequireRole2OrRole3", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "2") ||
        context.User.HasClaim(ClaimTypes.Role, "3")));

    // Penggabungan role 1, 2, and 3
    options.AddPolicy("RequireRole1Or2Or3", policy =>
    policy.RequireAssertion(context =>
        context.User.HasClaim(ClaimTypes.Role, "1") ||
        context.User.HasClaim(ClaimTypes.Role, "2") ||
        context.User.HasClaim(ClaimTypes.Role, "3")));
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
    var userManager = services.GetRequiredService<UserManager<AppUser>>();

    await context.Database.MigrateAsync();

    await SeedClassRoom.SeedData(context);
    await SeedToDoList.SeedData(context);
    //await SeedLesson.SeedData(context);
    await SeedUser.SeedData(context, userManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Terjadi kesalahan selama migrasi");
}

app.Run();