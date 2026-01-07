using AutoMapper;
using VShop.Application.Dtos.Categoria;
using VShop.Application.ViewModels.Productos;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{
    public class CategoriaMappingProfile : Profile
    {
        public CategoriaMappingProfile()
        {
            CreateMap<Categoria, CategoriaDto>().ReverseMap();
            CreateMap<Categoria, CategoriaViewModel>();

        }
    }
}
