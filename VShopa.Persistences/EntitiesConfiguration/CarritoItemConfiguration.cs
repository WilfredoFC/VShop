using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VShop.Domain.Entities;

namespace VShop.Persistences.EntitiesConfiguration
{
    public class CarritoItemConfiguration : IEntityTypeConfiguration<CarritoItem>
    {
        public void Configure(EntityTypeBuilder<CarritoItem> builder)
        {
            builder.ToTable("CarritoItems");

            builder.HasKey(ci => ci.Id);

            // Propiedades
            builder.Property(ci => ci.Cantidad)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(ci => ci.FechaAgregado)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relaciones

            builder.HasOne(ci => ci.Producto)
                .WithMany(p => p.CarritoItems)
                .HasForeignKey(ci => ci.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único compuesto (un usuario no puede tener el mismo producto dos veces en el carrito)
            builder.HasIndex(ci => new { ci.UsuarioId, ci.ProductoId })
                .IsUnique();

            // Índices adicionales
            builder.HasIndex(ci => ci.UsuarioId);
            builder.HasIndex(ci => ci.ProductoId);
        }
    }
}