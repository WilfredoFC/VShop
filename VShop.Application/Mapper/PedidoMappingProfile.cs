using AutoMapper;
using VShop.Application.Dtos.Pedido;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{
    public class PedidoMappingProfile : Profile
    {
        public PedidoMappingProfile()
        {
            // DTO → Entidad
            CreateMap<Pedido, PedidoDto>().ReverseMap();



        }
    }
}
