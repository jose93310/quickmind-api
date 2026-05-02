using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IDbConnectionFactory connectionFactory)
    {
        using var conn = await connectionFactory.CreateConnectionAsync();

        var exists = await conn.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM information_schema.tables WHERE table_name = 'users'");

        if (exists == 1)
        {
            Console.WriteLine("Base de datos ya inicializada.");
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("init.sql"));

        if (resourceName == null) return;

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync();

        var statements = SplitStatements(sql);
        foreach (var stmt in statements)
        {
            var trimmed = stmt.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                await conn.ExecuteAsync(trimmed);
            }
        }

        Console.WriteLine("Base de datos inicializada correctamente.");
    }

    private static List<string> SplitStatements(string sql)
    {
        var result = new List<string>();
        var lines = sql.Split('\n');
        var current = new System.Text.StringBuilder();
        var inFunction = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("--")) continue;

            if (trimmed.Contains("$$") && !inFunction)
                inFunction = true;
            else if (trimmed.EndsWith("$$") && inFunction)
            {
                current.AppendLine($"  {trimmed.TrimEnd(',')}");
                continue;
            }

            if (trimmed.EndsWith(";") && !inFunction)
            {
                current.AppendLine(trimmed[..^1]);
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.AppendLine(trimmed);
            }
        }

        return result;
    }
}
