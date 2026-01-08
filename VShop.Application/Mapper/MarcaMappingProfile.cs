using AutoMapper;
using VShop.Application.Dtos.Marca;
using VShop.Application.ViewModels.Marca;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{
    public class MarcaMappingProfile : Profile
    {
        public MarcaMappingProfile() 
        {
            CreateMap<Marca, MarcaDto>().ReverseMap();

            // DTO → Entidad
            CreateMap<MarcaDto, Marca>()
                .ForMember(dest => dest.Productos, opt => opt.Ignore()).ReverseMap();

            // DTO → ViewModel
            CreateMap<MarcaDto, MarcaViewModel>().ReverseMap();

            // ViewModel (Create) → DTO
            
        }
    }
}
