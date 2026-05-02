using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, name, display_name AS DisplayName, age_group AS AgeGroup, icon, is_active AS IsActive
            FROM categories WHERE is_active = true ORDER BY age_group, name
            """;
        return await conn.QueryAsync<Category>(sql);
    }

    public async Task<IEnumerable<Category>> GetByAgeGroupAsync(AgeGroup ageGroup)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, name, display_name AS DisplayName, age_group AS AgeGroup, icon, is_active AS IsActive
            FROM categories WHERE age_group = @AgeGroup AND is_active = true ORDER BY name
            """;
        return await conn.QueryAsync<Category>(sql, new { AgeGroup = (int)ageGroup });
    }

    public async Task<IEnumerable<Category>> GetByIdsAsync(List<int> ids)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, name, display_name AS DisplayName, age_group AS AgeGroup, icon, is_active AS IsActive
            FROM categories WHERE id = ANY(@Ids)
            """;
        return await conn.QueryAsync<Category>(sql, new { Ids = ids });
    }
}
