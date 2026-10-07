using AutoMapper;
using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Firmeza.Application.UseCases.Clientes;

public class CrearClienteUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService? _emailService;
    private readonly ILogger<CrearClienteUseCase>? _logger;
    private readonly IMapper? _mapper;

    public CrearClienteUseCase(
        IUnitOfWork unitOfWork,
        IEmailService? emailService = null,
        ILogger<CrearClienteUseCase>? logger = null,
        IMapper? mapper = null)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
        _mapper = mapper;
    }

    public CrearClienteUseCase(IUnitOfWork unitOfWork, IMapper? mapper)
        : this(unitOfWork, null, null, mapper)
    {
    }

    public async Task<ClienteDto> ExecuteAsync(CreateClienteDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = _mapper != null
            ? _mapper.Map<Cliente>(dto)
            : new Cliente
            {
                Id = Guid.NewGuid(),
                DocumentoIdentidad = dto.DocumentoIdentidad.Trim(),
                RazonSocial = dto.RazonSocial.Trim(),
                Telefono = dto.Telefono.Trim(),
                DireccionEnvio = dto.DireccionEnvio.Trim(),
                Email = dto.Email.Trim().ToLower()
            };

        if (cliente.Id == Guid.Empty)
        {
            cliente.Id = Guid.NewGuid();
        }

        await _unitOfWork.Clientes.AddAsync(cliente, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Envío defensivo de correo de bienvenida
        if (_emailService != null && !string.IsNullOrWhiteSpace(cliente.Email))
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(cliente.Email, cliente.RazonSocial, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "No se pudo enviar el correo de bienvenida al cliente {Email}", cliente.Email);
            }
        }

        return _mapper != null
            ? _mapper.Map<ClienteDto>(cliente)
            : new ClienteDto
            {
                Id = cliente.Id,
                DocumentoIdentidad = cliente.DocumentoIdentidad,
                RazonSocial = cliente.RazonSocial,
                Telefono = cliente.Telefono,
                DireccionEnvio = cliente.DireccionEnvio,
                Email = cliente.Email,
                TotalCompras = 0,
                MontoTotalComprado = 0m
            };
    }
}
