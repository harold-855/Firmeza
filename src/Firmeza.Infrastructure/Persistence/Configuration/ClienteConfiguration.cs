using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configuration;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.DocumentoIdentidad)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.DocumentoIdentidad)
            .IsUnique();

        builder.Property(c => c.RazonSocial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Telefono)
            .HasMaxLength(20);

        builder.Property(c => c.DireccionEnvio)
            .HasMaxLength(250);

        builder.Property(c => c.Email)
            .HasMaxLength(150);

        // Relación: Un cliente puede realizar muchas ventas
        builder.HasMany(c => c.Ventas)
            .WithOne(v => v.Cliente)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
