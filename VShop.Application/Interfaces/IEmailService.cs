using VShop.Application.Dtos.Email;
using VShop.Application.Dtos.Pedido;

namespace VShop.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
        Task SendReceiboAsync(PedidoDto pedido, string emailCliente, string nombreCliente);
        Task SendNotificacionNuevoPedidoAsync(PedidoDto pedido, string emailAdmin, string nombreCliente);
    }
}
