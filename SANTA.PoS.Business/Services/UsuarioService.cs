using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Business.Services;

public class UsuarioService(IUsuarioRepository repository, IMapper mapper)
{
    private readonly IUsuarioRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
    {
        var usuarios = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
    }

    public async Task<UsuarioDto> GetByIdAsync(string usuclave)
    {
        var usuario = await _repository.GetByIdAsync(usuclave)
            ?? throw new DomainException($"Usuario '{usuclave}' no encontrado.");
        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var existing = await _repository.GetByIdAsync(dto.Usuclave);
        if (existing is not null)
            throw new DomainException($"El usuario '{dto.Usuclave}' ya existe.");

        var usuario = new Usuario
        {
            Usuclave = dto.Usuclave,
            Nombre = dto.Nombre,
            APaterno = dto.APaterno,
            AMaterno = dto.AMaterno,
            Telefono = dto.Telefono,
            Pass = System.Text.Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword(dto.Password)),
            FechaAlta = DateTime.Now,
            FechaUltAct = DateTime.Now,
            Status = 1,
            Rol = dto.Rol
        };

        await _repository.CreateAsync(usuario);
        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task UpdateAsync(string usuclave, UpdateUsuarioDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var usuario = await _repository.GetByIdAsync(usuclave)
            ?? throw new DomainException($"Usuario '{usuclave}' no encontrado.");

        if (dto.Nombre is not null) usuario.Nombre = dto.Nombre;
        if (dto.APaterno is not null) usuario.APaterno = dto.APaterno;
        usuario.AMaterno = dto.AMaterno;
        if (dto.Telefono is not null) usuario.Telefono = dto.Telefono;
        usuario.FechaUltAct = DateTime.Now;

        await _repository.UpdateAsync(usuario);
    }

    public async Task ToggleStatusAsync(string usuclave)
    {
        var usuario = await _repository.GetByIdAsync(usuclave)
            ?? throw new DomainException($"Usuario '{usuclave}' no encontrado.");

        usuario.Status = usuario.Status == 1 ? 0 : 1;
        usuario.FechaUltAct = DateTime.Now;
        await _repository.UpdateAsync(usuario);
    }

    public async Task ChangePasswordAsync(string usuclave, ChangePasswordDto dto, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var usuario = await _repository.GetByIdAsync(usuclave)
            ?? throw new DomainException($"Usuario '{usuclave}' no encontrado.");

        if (!isAdmin)
        {
            if (dto.OldPassword is null)
                throw new DomainException("La contraseña actual es requerida.");

            var storedHash = usuario.Pass is not null
                ? System.Text.Encoding.UTF8.GetString(usuario.Pass)
                : null;

            if (storedHash is null || !BCrypt.Net.BCrypt.Verify(dto.OldPassword, storedHash))
                throw new DomainException("La contraseña actual es incorrecta.");

            if (dto.OldPassword == dto.NewPassword)
                throw new DomainException("La nueva contraseña no puede ser igual a la actual.");
        }

        usuario.Pass = System.Text.Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword(dto.NewPassword));
        usuario.FechaUltAct = DateTime.Now;
        await _repository.UpdateAsync(usuario);
    }

    public async Task<Usuario?> ValidateCredentialsAsync(string usuclave, string password)
    {
        var usuario = await _repository.GetByIdAsync(usuclave);
        if (usuario is null || usuario.Status == 0)
            return null;

        var storedHash = usuario.Pass is not null
            ? System.Text.Encoding.UTF8.GetString(usuario.Pass)
            : null;

        if (storedHash is null || !BCrypt.Net.BCrypt.Verify(password, storedHash))
            return null;

        return usuario;
    }
}
