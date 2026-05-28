using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VShop.Domain.Entities;

namespace VShop.Persistences.EntitiesConfiguration
{
    public class ProductoImagenConfiguration : IEntityTypeConfiguration<ProductoImagen>
    {
        public void Configure(EntityTypeBuilder<ProductoImagen> builder)
        {
            builder.ToTable("ProductoImagenes");

            builder.HasKey(pi => pi.Id);

            // Propiedades
            builder.Property(pi => pi.Datos)
                .HasColumnType("varbinary(max)");

            builder.Property(pi => pi.TipoContenido)
                .HasMaxLength(100);

            builder.Property(pi => pi.EsPrincipal)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pi => pi.Orden)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.EsActivo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(pi => pi.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(pi => pi.FechaActualizacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relación
            builder.HasOne(pi => pi.Producto)
                .WithMany(p => p.Imagenes)
                .HasForeignKey(pi => pi.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(pi => pi.ProductoId);
            builder.HasIndex(pi => new { pi.ProductoId, pi.EsPrincipal });
            builder.HasIndex(pi => new { pi.ProductoId, pi.Orden });
        }
    }
}