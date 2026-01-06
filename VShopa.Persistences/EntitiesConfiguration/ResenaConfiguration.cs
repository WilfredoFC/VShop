using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VShop.Domain.Entities;

namespace VShop.Persistences.EntitiesConfiguration
{
    public class ResenaConfiguration : IEntityTypeConfiguration<Resena>
    {
        public void Configure(EntityTypeBuilder<Resena> builder)
        {
            builder.ToTable("Resenas");

            builder.HasKey(r => r.Id);

            // Propiedades
            builder.Property(r => r.Calificacion)
                .IsRequired();

            builder.Property(r => r.Comentario)
                .HasMaxLength(1000);

            builder.Property(r => r.FechaResena)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(r => r.EsAprobado)
                .IsRequired()
                .HasDefaultValue(false);

            // Restricción para Calificación (1-5)
            builder.HasCheckConstraint("CK_Resenas_Calificacion",
                "[Calificacion] BETWEEN 1 AND 5");

            // Relaciones
            builder.HasOne(r => r.Producto)
                .WithMany(p => p.Resenas)
                .HasForeignKey(r => r.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(r => r.ProductoId);
            builder.HasIndex(r => r.UsuarioId);
            builder.HasIndex(r => r.EsAprobado);
            builder.HasIndex(r => new { r.ProductoId, r.EsAprobado });
            builder.HasIndex(r => new { r.UsuarioId, r.ProductoId })
                .IsUnique(); // Un usuario solo puede hacer una reseña por producto
        }
    }
}