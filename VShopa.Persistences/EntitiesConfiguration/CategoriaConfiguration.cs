using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VShop.Domain.Entities;

namespace VShop.Persistences.EntitiesConfiguration
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            // Tabla
            builder.ToTable("Categorias");

            // Clave primaria
            builder.HasKey(c => c.Id);

            // Propiedades
            builder.Property(c => c.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Descripcion)
                .HasMaxLength(500);

            builder.Property(c => c.Tipo)
                .HasMaxLength(50)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(c => c.EsActivo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.FechaActualizacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Restricción para Tipo
            builder.HasCheckConstraint("CK_Categorias_Tipo",
                "[Tipo] IN ('Personal', 'Hogar')");

            // Propiedades de navegación
            builder.HasMany(c => c.Productos)
                .WithOne(p => p.Categoria)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}