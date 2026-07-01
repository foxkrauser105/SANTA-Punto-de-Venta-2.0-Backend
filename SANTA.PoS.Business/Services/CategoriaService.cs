using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Business.Services;

public class CategoriaService(ICategoriaRepository repository, IMapper mapper)
{
    private readonly ICategoriaRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<CategoriaDto>> GetAllAsync()
    {
        var categorias = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task<CategoriaDto> CreateAsync(string nombre)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre, nameof(nombre));

        var existing = await _repository.GetByIdAsync(nombre);
        if (existing is not null)
            throw new DomainException($"La categoría '{nombre}' ya existe.");

        var categoria = new Categoria { Nombre = nombre };
        await _repository.CreateAsync(categoria);
        return _mapper.Map<CategoriaDto>(categoria);
    }

    public async Task DeleteAsync(string nombre)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre, nameof(nombre));

        var existing = await _repository.GetByIdAsync(nombre);
        if (existing is null)
            throw new DomainException($"La categoría '{nombre}' no existe.");

        await _repository.DeleteAsync(nombre);
    }
}
