using Firmeza.Infrastructure;
using Firmeza.Infrastructure.Identity;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar Servicios de Infraestructura (EF Core, Repositorios, Identity) y Casos de Uso de Aplicación
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Autenticación basada en JWT Bearer y Políticas de Autorización RBAC (Administrador / Cliente)
builder.Services.AddJwtAuthenticationAndAuthorization(builder.Configuration);

// 3. Controladores de API REST
builder.Services.AddControllers();

// 4. Documentación y Pruebas con Swagger (Swashbuckle) con soporte JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Firmeza API",
        Version = "v1",
        Description = "API RESTful para el Sistema de Gestión y Despacho de Materiales de Construcción Firmeza.",
        Contact = new OpenApiContact
        {
            Name = "Equipo de Desarrollo Firmeza",
            Email = "contacto@firmeza.com"
        }
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Autenticación JWT Bearer.\n\nIngrese 'Bearer' seguido de un espacio y su token JWT.\n\nEjemplo: `Bearer eyJhbGciOiJIUzI1NiIsIn...`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

// 5. Configuración de CORS para clientes externos (Angular SPA, Blazor, etc.)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:5281")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 6. Aplicar migraciones automáticas y sembrado si es necesario
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
        logger.LogError(ex, "Ocurrió un error al verificar o sembrar la base de datos desde Firmeza.Api.");
    }
}

// 7. Pipeline HTTP y Swagger UI
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Firmeza API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Firmeza API - Swagger UI";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowClientApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
