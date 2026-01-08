using AutoMapper;
using VShop.Application.Dtos.InventarioMovimiento;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{
    public class InventarioMovimientoMappingProfile : Profile
    {
        public InventarioMovimientoMappingProfile()
        {
            // DTO → Entidad
            CreateMap<InventarioMovimiento, InventarioMovimientoDto>().ReverseMap();
        }
    }
}
