using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VShop.Domain.Entities;

namespace VShop.Persistences.EntitiesConfiguration
{
    public class PedidoDetalleConfiguration : IEntityTypeConfiguration<PedidoDetalle>
    {
        public void Configure(EntityTypeBuilder<PedidoDetalle> builder)
        {
            builder.ToTable("PedidoDetalles");

            builder.HasKey(pd => pd.Id);

            // Propiedades
            builder.Property(pd => pd.Cantidad)
                .IsRequired();

            builder.Property(pd => pd.PrecioUnitario)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(pd => pd.Subtotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Relaciones
            builder.HasOne(pd => pd.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(pd => pd.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pd => pd.Producto)
                .WithMany(p => p.PedidoDetalles)
                .HasForeignKey(pd => pd.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(pd => pd.PedidoId);
            builder.HasIndex(pd => pd.ProductoId);
            builder.HasIndex(pd => new { pd.PedidoId, pd.ProductoId });
        }
    }
}