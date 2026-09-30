using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence;

// Aplicamos el "Primary Constructor" directamente en la declaración de la clase
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Las advertencias desaparecerán en cuanto agregues tablas aquí más adelante
    }
}   