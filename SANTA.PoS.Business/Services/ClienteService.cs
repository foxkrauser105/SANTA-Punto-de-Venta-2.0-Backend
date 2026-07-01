using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Business.Services;

public class ClienteService(IClienteRepository repository, IMapper mapper)
{
    private readonly IClienteRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<ClienteDto>> GetAllAsync()
    {
        var clientes = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
    }

    public async Task<ClienteDto> GetByIdAsync(int numcliente)
    {
        var cliente = await _repository.GetByIdAsync(numcliente)
            ?? throw new DomainException($"Cliente con número '{numcliente}' no encontrado.");
        return _mapper.Map<ClienteDto>(cliente);
    }

    public async Task<int> GetNextFolioAsync() => await _repository.GetNextFolioAsync();

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.Telefono.Length < 10)
            throw new DomainException("El teléfono debe tener al menos 10 dígitos.");

        var existing = await _repository.GetByIdAsync(dto.Numcliente);
        if (existing is not null)
            throw new DomainException($"Ya existe un cliente con el número '{dto.Numcliente}'.");

        var cliente = _mapper.Map<Cliente>(dto);
        await _repository.CreateAsync(cliente);
        return _mapper.Map<ClienteDto>(cliente);
    }

    public async Task UpdateAsync(int numcliente, UpdateClienteDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var cliente = await _repository.GetByIdAsync(numcliente)
            ?? throw new DomainException($"Cliente con número '{numcliente}' no encontrado.");

        if (dto.Telefono is not null && dto.Telefono.Length < 10)
            throw new DomainException("El teléfono debe tener al menos 10 dígitos.");

        if (dto.Nombre is not null) cliente.Nombre = dto.Nombre;
        if (dto.APaterno is not null) cliente.APaterno = dto.APaterno;
        cliente.AMaterno = dto.AMaterno;
        if (dto.Calle is not null) cliente.Calle = dto.Calle;
        if (dto.NumeroExt.HasValue) cliente.NumeroExt = dto.NumeroExt.Value;
        cliente.NumeroInt = dto.NumeroInt;
        if (dto.Colonia is not null) cliente.Colonia = dto.Colonia;
        if (dto.Telefono is not null) cliente.Telefono = dto.Telefono;
        cliente.UsuclaveUltAct = dto.UsuclaveUltAct;

        await _repository.UpdateAsync(cliente);
    }
}
