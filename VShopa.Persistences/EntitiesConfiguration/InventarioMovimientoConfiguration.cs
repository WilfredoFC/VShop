using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VShop.Domain.Entities;

namespace VShop.Persistences.EntitiesConfiguration
{
    public class InventarioMovimientoConfiguration : IEntityTypeConfiguration<InventarioMovimiento>
    {
        public void Configure(EntityTypeBuilder<InventarioMovimiento> builder)
        {
            builder.ToTable("InventarioMovimientos");

            builder.HasKey(im => im.Id);

            // Propiedades
            builder.Property(im => im.TipoMovimiento)
                .HasMaxLength(50)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(im => im.Cantidad)
                .IsRequired();

            builder.Property(im => im.StockAnterior)
                .IsRequired();

            builder.Property(im => im.StockNuevo)
                .IsRequired();

            builder.Property(im => im.Observaciones)
                .HasMaxLength(500);

            builder.Property(im => im.FechaMovimiento)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Restricción para TipoMovimiento
            builder.HasCheckConstraint("CK_InventarioMovimientos_TipoMovimiento",
                "[TipoMovimiento] IN ('Entrada', 'Salida', 'Ajuste', 'Venta', 'Devolucion')");

            // Relaciones
            builder.HasOne(im => im.Producto)
                .WithMany(p => p.InventarioMovimientos)
                .HasForeignKey(im => im.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(im => im.ProductoId);
            builder.HasIndex(im => im.TipoMovimiento);
            builder.HasIndex(im => im.FechaMovimiento);
            builder.HasIndex(im => new { im.ProductoId, im.FechaMovimiento });
        }
    }
}