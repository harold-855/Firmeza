using AutoMapper;
using Firmeza.Application.DTOS.Productos;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

public class ProductoMappingProfile : Profile
{
    public ProductoMappingProfile()
    {
        // Entity -> DTO
        CreateMap<Producto, ProductoDto>()
            .ForMember(dest => dest.TotalVentasAsociadas, opt => opt.MapFrom(src => src.DetallesVenta.Count));

        // CreateDto -> Entity
        CreateMap<CreateProductoDto, Producto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre.Trim()))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion.Trim()))
            .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida.Trim()))
            .ForMember(dest => dest.DetallesVenta, opt => opt.Ignore());

        // UpdateDto -> Entity
        CreateMap<UpdateProductoDto, Producto>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre.Trim()))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion.Trim()))
            .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida.Trim()))
            .ForMember(dest => dest.DetallesVenta, opt => opt.Ignore());
    }
}
