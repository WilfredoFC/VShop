using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VShop.Domain.Entities;

namespace VShop.Persistences.EntitiesConfiguration
{
    public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
    {
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            builder.ToTable("Pedidos");

            builder.HasKey(p => p.Id);

            // Propiedades
            builder.Property(p => p.NumeroPedido)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Estado)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("Pendiente")
                .HasConversion<string>();

            builder.Property(p => p.Subtotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Impuestos)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Total)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.MetodoPago)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.DireccionEnvio)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(p => p.Ciudad)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.TelefonoContacto)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Notas)
                .HasMaxLength(1000);

            builder.Property(p => p.FechaPedido)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.EsActivo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.FechaActualizacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Restricción para Estado
            builder.HasCheckConstraint("CK_Pedidos_Estado",
                "[Estado] IN ('Pendiente', 'Confirmado', 'EnProceso', 'Enviado', 'Entregado', 'Cancelado')");

            

            // Propiedades de navegación
            builder.HasMany(p => p.Detalles)
                .WithOne(pd => pd.Pedido)
                .HasForeignKey(pd => pd.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(p => p.NumeroPedido)
                .IsUnique();

            builder.HasIndex(p => p.UsuarioId);
            builder.HasIndex(p => p.Estado);
            builder.HasIndex(p => p.FechaPedido);
            builder.HasIndex(p => new { p.UsuarioId, p.FechaPedido });
        }
    }
}