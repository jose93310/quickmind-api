using QuickMind.Application.DTOs;

namespace QuickMind.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<LoginResponseDto> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> GuestLoginAsync(string nickname);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
}
