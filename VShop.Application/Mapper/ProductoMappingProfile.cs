using AutoMapper;
using VShop.Application.Dtos.Producto;
using VShop.Application.ViewModels.Productos;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{

    public class ProductoMappingProfile : Profile
    {
        public ProductoMappingProfile()
        {
            // Productos
            CreateMap<Producto, ProductoDto>().ReverseMap();
            CreateMap<Producto, ProductoViewModel>()
                .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria.Nombre))
                .ForMember(dest => dest.Marca, opt => opt.MapFrom(src => src.Marca != null ? src.Marca.Nombre : "Sin marca"))
                .ForMember(dest => dest.ImagenPrincipal, opt => opt.MapFrom(src => src.Imagenes.FirstOrDefault().UrlImagen));
        }
    }
}
