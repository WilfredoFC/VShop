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
            CreateMap<Marca, MarcaViewModel>();
        }
    }
}
