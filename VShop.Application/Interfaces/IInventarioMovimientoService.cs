using VShop.Application.Dtos.InventarioMovimiento;
using VShop.Domain.Entities;

namespace VShop.Application.Interfaces
{
    public interface IInventarioMovimientoService : IBaseServices<InventarioMovimiento, InventarioMovimientoDto>
    {
    }
}