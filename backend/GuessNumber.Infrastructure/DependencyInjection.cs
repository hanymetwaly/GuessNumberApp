using GuessNumber.Application.Interfaces;
using GuessNumber.Application.Services;
using GuessNumber.Infrastructure.Persistence;
using GuessNumber.Infrastructure.Repositories;
using GuessNumber.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GuessNumber.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // Options
        services.Configure<JwtSettings>(config.GetSection("Jwt"));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameRepository, GameRepository>();

        // Security
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGameService, GameService>();

        return services;
    }
}