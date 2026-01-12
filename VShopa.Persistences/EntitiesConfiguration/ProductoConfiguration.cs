using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VShop.Domain.Entities;

namespace VShop.Persistences.EntitiesConfiguration
{
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("Productos");

            builder.HasKey(p => p.Id);

            // Propiedades
            builder.Property(p => p.Nombre)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Descripcion)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.Precio)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.PrecioDescuento)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.SKU)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Stock)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.StockMinimo)
                .IsRequired()
                .HasDefaultValue(10);

            builder.Property(p => p.EsActivo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.FechaActualizacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relaciones
            builder.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.Marca)
                .WithMany(m => m.Productos)
                .HasForeignKey(p => p.MarcaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Propiedades de navegación
            builder.HasMany(p => p.Imagenes)
                .WithOne(pi => pi.Producto)
                .HasForeignKey(pi => pi.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Resenas)
                .WithOne(r => r.Producto)
                .HasForeignKey(r => r.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.CarritoItems)
                .WithOne(ci => ci.Producto)
                .HasForeignKey(ci => ci.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.InventarioMovimientos)
                .WithOne(im => im.Producto)
                .HasForeignKey(im => im.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(p => p.SKU)
                .IsUnique();

            builder.HasIndex(p => p.CategoriaId);
            builder.HasIndex(p => p.MarcaId);
            builder.HasIndex(p => p.EsActivo);
            builder.HasIndex(p => p.Precio);
            builder.HasIndex(p => new { p.EsActivo, p.Stock });

            // Ignorar propiedades calculadas
            builder.Ignore(p => p.PrecioFinal);
            builder.Ignore(p => p.TieneDescuento);
            builder.Ignore(p => p.StockBajo);
        }
    }
}