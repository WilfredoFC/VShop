using AutoMapper;
using VShop.Application.Dtos.CarritoItem;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{
    public class CarritoItemMappingProfile : Profile
    {
        public CarritoItemMappingProfile()
        {
            // DTO → Entidad // Entidad → DTO
            CreateMap<CarritoItem, CarritoItemDto>().ReverseMap();

        }
    }
}
