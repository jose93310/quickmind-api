using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, email, nickname, name, country, city, birth_date AS BirthDate,
                   gender, avatar_path AS AvatarPath, password_hash AS PasswordHash,
                   is_guest AS IsGuest, created_at AS CreatedAt
            FROM users WHERE id = @Id
            """;
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, email, nickname, name, country, city, birth_date AS BirthDate,
                   gender, avatar_path AS AvatarPath, password_hash AS PasswordHash,
                   is_guest AS IsGuest, created_at AS CreatedAt
            FROM users WHERE email = @Email
            """;
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByNicknameAsync(string nickname)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, email, nickname, name, country, city, birth_date AS BirthDate,
                   gender, avatar_path AS AvatarPath, password_hash AS PasswordHash,
                   is_guest AS IsGuest, created_at AS CreatedAt
            FROM users WHERE nickname = @Nickname
            """;
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Nickname = nickname });
    }

    public async Task<IEnumerable<User>> SearchByNicknameAsync(string query, Guid excludeUserId, int limit = 20)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, email, nickname, name, country, city, birth_date AS BirthDate,
                   gender, avatar_path AS AvatarPath, password_hash AS PasswordHash,
                   is_guest AS IsGuest, created_at AS CreatedAt
            FROM users 
            WHERE nickname ILIKE @Query 
              AND id != @ExcludeUserId
              AND is_guest = false
            ORDER BY nickname
            LIMIT @Limit
            """;
        return await conn.QueryAsync<User>(sql, new { Query = $"%{query}%", ExcludeUserId = excludeUserId, Limit = limit });
    }

    public async Task<User> CreateAsync(User user)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO users (id, email, nickname, name, country, city, birth_date,
                               gender, avatar_path, password_hash, is_guest, created_at)
            VALUES (@Id, @Email, @Nickname, @Name, @Country, @City, @BirthDate,
                    @Gender, @AvatarPath, @PasswordHash, @IsGuest, @CreatedAt)
            RETURNING id
            """;
        user.Id = await conn.QuerySingleAsync<Guid>(sql, user);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            UPDATE users SET name = @Name, country = @Country, city = @City,
                             avatar_path = @AvatarPath
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, user);
    }
}
