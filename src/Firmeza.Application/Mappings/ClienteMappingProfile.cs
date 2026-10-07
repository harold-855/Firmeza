using AutoMapper;
using Firmeza.Application.DTOS.Clientes;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

public class ClienteMappingProfile : Profile
{
    public ClienteMappingProfile()
    {
        // Entity -> DTO
        CreateMap<Cliente, ClienteDto>()
            .ForMember(dest => dest.TotalCompras, opt => opt.MapFrom(src => src.Ventas.Count))
            .ForMember(dest => dest.MontoTotalComprado, opt => opt.MapFrom(src => src.Ventas.Sum(v => (decimal?)v.Total) ?? 0m));

        // CreateDto -> Entity
        CreateMap<CreateClienteDto, Cliente>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.DocumentoIdentidad, opt => opt.MapFrom(src => src.DocumentoIdentidad.Trim()))
            .ForMember(dest => dest.RazonSocial, opt => opt.MapFrom(src => src.RazonSocial.Trim()))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Trim()))
            .ForMember(dest => dest.DireccionEnvio, opt => opt.MapFrom(src => src.DireccionEnvio.Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLower()))
            .ForMember(dest => dest.Ventas, opt => opt.Ignore());

        // UpdateDto -> Entity
        CreateMap<UpdateClienteDto, Cliente>()
            .ForMember(dest => dest.DocumentoIdentidad, opt => opt.MapFrom(src => src.DocumentoIdentidad.Trim()))
            .ForMember(dest => dest.RazonSocial, opt => opt.MapFrom(src => src.RazonSocial.Trim()))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Trim()))
            .ForMember(dest => dest.DireccionEnvio, opt => opt.MapFrom(src => src.DireccionEnvio.Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLower()))
            .ForMember(dest => dest.Ventas, opt => opt.Ignore());
    }
}
