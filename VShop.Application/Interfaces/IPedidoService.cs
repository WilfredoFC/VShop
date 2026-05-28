using VShop.Application.Dtos.Pedido;
using VShop.Domain.Entities;

namespace VShop.Application.Interfaces
{
    public interface IPedidoService : IBaseServices<Pedido, PedidoDto>
    {
        Task<PedidoDto?> GetPedidoByNumeroAsync(string numeroPedido);
        Task<PedidoDto?> GetPedidoWithDetailsAsync(int id);
        Task<List<PedidoDto>> GetPedidosByUsuarioAsync(string usuarioId);
        Task<PedidoDto?> UpdateEstadoAsync(int id, string nuevoEstado);
    }
}