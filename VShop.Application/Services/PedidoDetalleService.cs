using AutoMapper;
using VShop.Application.Dtos.PedidoDetalle;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class PedidoDetalleService : BaseServices<PedidoDetalle, PedidoDetalleDto>, IPedidoDetalleService
    {
        public PedidoDetalleService(IMapper mapper, IBaseRepository<PedidoDetalle> _baseRepository) : base(mapper, _baseRepository)
        {
        }
    }
}
