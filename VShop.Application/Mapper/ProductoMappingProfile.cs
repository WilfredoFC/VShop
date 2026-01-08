using AutoMapper;
using VShop.Application.Dtos.Producto;
using VShop.Application.ViewModels.Producto;
using VShop.Domain.Entities;

namespace VShop.Application.Mapper
{

    public class ProductoMappingProfile : Profile
    {
        public ProductoMappingProfile()
        {
            // Productos
            // Entidad → DTO
            CreateMap<Producto, ProductoDto>().ReverseMap();

            // DTO → Entidad
            CreateMap<ProductoDto, Producto>()
                .ForMember(dest => dest.Categoria, opt => opt.Ignore())
                .ForMember(dest => dest.Marca, opt => opt.Ignore())
                .ForMember(dest => dest.Imagenes, opt => opt.Ignore())
                .ForMember(dest => dest.PedidoDetalles, opt => opt.Ignore())
                .ForMember(dest => dest.Resenas, opt => opt.Ignore())
                .ForMember(dest => dest.CarritoItems, opt => opt.Ignore())
                .ForMember(dest => dest.InventarioMovimientos, opt => opt.Ignore()).ReverseMap();

            // DTO → ViewModel (para Index)
            CreateMap<ProductoDto, ProductoViewModel>()
                .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria))
                .ForMember(dest => dest.Marca, opt => opt.MapFrom(src => src.Marca))
                .ForMember(dest => dest.ImagenPrincipal, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoStock, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoStockClase, opt => opt.Ignore())
                .ForMember(dest => dest.TieneDescuento,
                    opt => opt.MapFrom(src => src.PrecioDescuento.HasValue && src.PrecioDescuento < src.Precio))
                .ForMember(dest => dest.PrecioFinal,
                    opt => opt.MapFrom(src => src.PrecioDescuento.HasValue && src.PrecioDescuento < src.Precio
                        ? src.PrecioDescuento.Value : src.Precio)).ReverseMap();

           
        }
    }
}
