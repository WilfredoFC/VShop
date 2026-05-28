using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VShop.Domain.Entities;

namespace VShop.Persistences.EntitiesConfiguration
{
    public class MarcaConfiguration : IEntityTypeConfiguration<Marca>
    {
        public void Configure(EntityTypeBuilder<Marca> builder)
        {
            builder.ToTable("Marcas");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(m => m.Descripcion)
                .HasMaxLength(500);

            builder.Property(m => m.EsActivo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(m => m.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(m => m.FechaActualizacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relaciones
            builder.HasMany(m => m.Productos)
                .WithOne(p => p.Marca)
                .HasForeignKey(p => p.MarcaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Índices
            builder.HasIndex(m => m.Nombre)
                .IsUnique();

            builder.HasIndex(m => m.EsActivo);
        }
    }
}