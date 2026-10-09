using Firmeza.Infrastructure;
using Firmeza.Infrastructure.Identity;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios de infraestructura (DbContext, Identity, AuthService)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Autenticación JWT y Autorización RBAC para la SPA y APIs
builder.Services.AddJwtAuthenticationAndAuthorization(builder.Configuration);

// Configuración de la cookie de autenticación para Razor
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:5281", "https://localhost:7091")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Aplicar migraciones automáticas y ejecutar el sembrado de roles/datos iniciales
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        await IdentitySeeder.SeedRolesAndAdminAsync(roleManager, userManager);

        await DataSeeder.SeedSampleDataAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar migraciones o sembrar datos iniciales en la base de datos.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowClientApp");
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

// Redirigir rutas de Angular SPA bajo /spa
app.MapFallbackToFile("/spa", "spa/index.html");
app.MapFallbackToFile("/spa/{*path:nonfile}", "spa/index.html");

app.Run();