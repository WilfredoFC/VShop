using AutoMapper;
using VShop.Application.Dtos.ProductoImagen;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{
    public class ProductoImagenMappingProfile : Profile
    {
        public ProductoImagenMappingProfile()
        {
            CreateMap<ProductoImagen, ProductoImagenDto>().ReverseMap();
        }
    }
}
