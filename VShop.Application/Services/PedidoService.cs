using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VShop.Application.Dtos.Pedido;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class PedidoService : BaseServices<Pedido, PedidoDto>, IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMapper _mapper;

        public PedidoService(IMapper mapper, IPedidoRepository _baseRepository) : base(mapper, _baseRepository)
        {
            _pedidoRepository = _baseRepository;
            _mapper = mapper;
        }

        public async Task<PedidoDto?> GetPedidoWithDetailsAsync(int id)
        {
            var pedido = await _pedidoRepository
                .GetAllQueryWithInclude(new List<string> { "Detalles", "Detalles.Producto" })
                .FirstOrDefaultAsync(p => p.Id == id);

            return pedido != null ? _mapper.Map<PedidoDto>(pedido) : null;
        }

        public async Task<PedidoDto?> GetPedidoByNumeroAsync(string numeroPedido)
        {
            var pedido = await _pedidoRepository
                .GetAllQueryWithInclude(new List<string> { "Detalles", "Detalles.Producto" })
                .FirstOrDefaultAsync(p => p.NumeroPedido == numeroPedido);

            return pedido != null ? _mapper.Map<PedidoDto>(pedido) : null;
        }

        public async Task<List<PedidoDto>> GetPedidosByUsuarioAsync(string usuarioId)
        {
            var pedidos = await _pedidoRepository
                .GetAllQueryWithInclude(new List<string> { "Detalles", "Detalles.Producto" })
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return _mapper.Map<List<PedidoDto>>(pedidos);
        }

        public async Task<PedidoDto?> UpdateEstadoAsync(int id, string nuevoEstado)
        {
            var pedido = await _pedidoRepository.GetEntityByIdAsync(id);
            if (pedido == null)
                return null;

            pedido.Estado = nuevoEstado;
            var updated = await _pedidoRepository.UpdateEntityAsync(id, pedido);
            return _mapper.Map<PedidoDto>(updated);
        }
    }
}
