using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace QuickMind.Infrastructure.Auth;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string nickname, bool isGuest);
    byte[] HashPassword(string password);
    bool VerifyPassword(string password, byte[] storedHash);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(Guid userId, string email, string nickname, bool isGuest)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("nickname", nickname),
            new Claim("is_guest", isGuest.ToString().ToLower())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public byte[] HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var combined = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);
        return combined;
    }

    public bool VerifyPassword(string password, byte[] storedHash)
    {
        var salt = storedHash[..16];
        var originalHash = storedHash[16..];
        using var sha256 = SHA256.Create();
        var testHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return originalHash.SequenceEqual(testHash);
    }
}
