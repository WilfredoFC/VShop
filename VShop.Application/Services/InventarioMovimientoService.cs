using AutoMapper;
using VShop.Application.Dtos.InventarioMovimiento;
using VShop.Application.Interfaces;
using VShop.Domain.Entities;
using VShop.Domain.Interfaces;

namespace VShop.Application.Services
{
    public class InventarioMovimientoService : BaseServices<InventarioMovimiento, InventarioMovimientoDto>, IInventarioMovimientoService
    {
        public InventarioMovimientoService(IMapper mapper, IInventarioMovimientoRepository _baseRepository) : base(mapper, _baseRepository)
        {
        }
    }
}
