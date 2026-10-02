using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configuration;

public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> builder)
    {
        builder.ToTable("VentaDetalles");

        builder.HasKey(vd => vd.Id);

        builder.Property(vd => vd.Cantidad)
            .IsRequired();

        builder.Property(vd => vd.PrecioAplicado)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Ignore(vd => vd.Subtotal);

        builder.Property(vd => vd.VentaId)
            .IsRequired();

        builder.Property(vd => vd.ProductoId)
            .IsRequired();

        // Relación con Venta (Muchos a Uno)
        builder.HasOne(vd => vd.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(vd => vd.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Producto (Muchos a Uno)
        builder.HasOne(vd => vd.Producto)
            .WithMany(p => p.DetallesVenta)
            .HasForeignKey(vd => vd.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
