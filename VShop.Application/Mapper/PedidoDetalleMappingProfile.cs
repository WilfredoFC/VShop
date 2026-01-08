using AutoMapper;
using VShop.Application.Dtos.PedidoDetalle;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{
    public class PedidoDetalleMappingProfile : Profile
    {
        public PedidoDetalleMappingProfile()
        {
            // DTO → Entidad
            CreateMap<PedidoDetalle, PedidoDetalleDto>().ReverseMap();
        }
    }
}
