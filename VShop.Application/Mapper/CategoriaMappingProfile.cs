using AutoMapper;
using VShop.Application.Dtos.Categoria;
using VShop.Application.ViewModels.Categoria;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{
    public class CategoriaMappingProfile : Profile
    {
        public CategoriaMappingProfile()
        {
            // Entidad → DTO
            CreateMap<Categoria, CategoriaDto>()
                .ForMember(dest => dest.TotalProductos,
                    opt => opt.MapFrom(src => src.Productos.Count)).ReverseMap();

            // DTO → Entidad
            CreateMap<CategoriaDto, Categoria>()
                .ForMember(dest => dest.Productos, opt => opt.Ignore()).ReverseMap();

            // DTO → ViewModel
            

            // ViewModel (Create) → DTO
            CreateMap<CategoriaCreateViewModel, CategoriaDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TotalProductos, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Productos, opt => opt.Ignore())
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => DateTime.UtcNow)).ReverseMap();

            

        }
    }
}
