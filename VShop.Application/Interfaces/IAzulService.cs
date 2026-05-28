using Microsoft.AspNetCore.Http;
using VShop.Application.Dtos.Pago;
using VShop.Application.Dtos.Pedido;

namespace VShop.Application.Interfaces
{
    public interface IAzulService
    {
        Task<AzulSolicitudResponse> SolicitarTokenAsync(PedidoDto pedido);
        Task<bool> ValidarNotificacionAsync(IFormCollection formData); // opcional, para verificar firma
        AzulNotificacionResult ProcesarNotificacion(IFormCollection formData);
    }
}
