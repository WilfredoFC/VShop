using AutoMapper;
using VShop.Application.Dtos.Pedido;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class PedidoService : BaseServices<Pedido, PedidoDto>, IPedidoService
    {
        public PedidoService(IMapper mapper, IPedidoRepository _baseRepository) : base(mapper, _baseRepository)
        {
        }
    }
}
