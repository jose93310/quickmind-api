using QuickMind.Application.DTOs;
using QuickMind.Application.Services;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Auth;

namespace QuickMind.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepo, IJwtService jwtService)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");

        var valid = _jwtService.VerifyPassword(dto.Password, Convert.FromBase64String(user.PasswordHash));
        if (!valid)
            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Nickname, user.IsGuest);
        return new LoginResponseDto(user.Id, user.Nickname, user.Email, token, user.IsGuest);
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userRepo.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("El email ya está registrado");

        var byNick = await _userRepo.GetByNicknameAsync(dto.Nickname);
        if (byNick != null)
            throw new InvalidOperationException("El nickname ya está en uso");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            Nickname = dto.Nickname,
            PasswordHash = Convert.ToBase64String(_jwtService.HashPassword(dto.Password)),
            IsGuest = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.CreateAsync(user);

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Nickname, false);
        return new LoginResponseDto(user.Id, user.Nickname, user.Email, token, false);
    }

    public async Task<LoginResponseDto> GuestLoginAsync(string nickname)
    {
        var guestId = Guid.NewGuid();
        var guestNickname = $"{nickname}_{Random.Shared.Next(1000, 9999)}";

        var user = new User
        {
            Id = guestId,
            Email = $"guest_{guestId:N}@quickmind.local",
            Nickname = guestNickname,
            PasswordHash = string.Empty,
            IsGuest = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.CreateAsync(user);

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Nickname, true);
        return new LoginResponseDto(user.Id, user.Nickname, user.Email, token, true);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserDto(user.Id, user.Nickname, user.Name, user.Country,
            user.City, user.BirthDate, user.Gender, user.AvatarPath);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        user.Name = dto.Name;
        user.Country = dto.Country;
        user.City = dto.City;
        await _userRepo.UpdateAsync(user);

        return new UserDto(user.Id, user.Nickname, user.Name, user.Country,
            user.City, user.BirthDate, user.Gender, user.AvatarPath);
    }
}
