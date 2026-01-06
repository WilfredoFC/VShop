using VShop.Application.Dtos.Pedido;
using VShop.Domain.Entities;

namespace VShop.Application.Interfaces
{
    public interface IPedidoService : IBaseServices<Pedido, PedidoDto>
    {
    }
}