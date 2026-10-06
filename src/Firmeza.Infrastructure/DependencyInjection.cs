using Firmeza.Application;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Infrastructure.Identity;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Repositories;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Repositories & Unit of Work (Clean Architecture)
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Infrastructure external/technical services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IExcelImportService, ExcelImportService>();

        // Application Use Cases & Services
        services.AddApplicationServices();

        return services;
    }
}
