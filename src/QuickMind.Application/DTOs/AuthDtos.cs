namespace QuickMind.Application.DTOs;

public record LoginDto(string Email, string Password);

public record RegisterDto(string Email, string Nickname, string Password);

public record LoginResponseDto(Guid UserId, string Nickname, string Email, string Token, bool IsGuest);

public record UserDto(Guid Id, string Nickname, string? Name, string? Country, string? City, DateTime? BirthDate, string? Gender, string? AvatarPath);

public record UpdateProfileDto(string? Name, string? Country, string? City);
