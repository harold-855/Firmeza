using AutoMapper;
using Firmeza.Application.DTOS.Ventas;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

public class VentaMappingProfile : Profile
{
    public VentaMappingProfile()
    {
        // Venta -> VentaDto
        CreateMap<Venta, VentaDto>()
            .ForMember(dest => dest.ClienteRazonSocial, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.RazonSocial : "Cliente General"))
            .ForMember(dest => dest.ClienteDocumento, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.DocumentoIdentidad : string.Empty))
            .ForMember(dest => dest.ClienteTelefono, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Telefono : string.Empty))
            .ForMember(dest => dest.ClienteDireccion, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.DireccionEnvio : string.Empty))
            .ForMember(dest => dest.ClienteEmail, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Email : string.Empty))
            .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles))
            .ForMember(dest => dest.RutaArchivoRecibo, opt => opt.Ignore());

        // VentaDetalle -> VentaDetalleDto
        CreateMap<VentaDetalle, VentaDetalleDto>()
            .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : "Material General"))
            .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.UnidadMedida : "UND"));

        // CreateVentaDto -> Venta
        CreateMap<CreateVentaDto, Venta>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.FechaVenta, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.EstadoDespacho, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.EstadoDespacho) ? "Pendiente" : src.EstadoDespacho.Trim()))
            .ForMember(dest => dest.Cliente, opt => opt.Ignore())
            .ForMember(dest => dest.Detalles, opt => opt.Ignore())
            .ForMember(dest => dest.Total, opt => opt.Ignore());
    }
}
