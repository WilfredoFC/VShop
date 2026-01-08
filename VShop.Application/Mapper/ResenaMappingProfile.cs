using AutoMapper;
using VShop.Application.Dtos.Resena;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{
    public class ResenaMappingProfile : Profile
    {
        public ResenaMappingProfile()
        {
            // DTO → Entidad
            CreateMap<Resena, ResenaDto>().ReverseMap();
        }
    }
}
