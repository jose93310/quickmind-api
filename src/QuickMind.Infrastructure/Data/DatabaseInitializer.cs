using System.Data;
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
            Console.WriteLine("Base de datos ya inicializada. Ejecutando migraciones...");
            await RunMigrationsAsync(conn);
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

    private static async Task RunMigrationsAsync(IDbConnection conn)
    {
        // Migration: Add is_public and scheduled_start columns to games table
        const string migration1 = """
            ALTER TABLE games 
            ADD COLUMN IF NOT EXISTS is_public BOOLEAN DEFAULT FALSE,
            ADD COLUMN IF NOT EXISTS scheduled_start TIMESTAMP;
            """;
        await conn.ExecuteAsync(migration1);

        // Migration: Create index for public games query
        const string migration2 = """
            CREATE INDEX IF NOT EXISTS idx_games_public ON games(is_public, status, scheduled_start);
            """;
        await conn.ExecuteAsync(migration2);

        // Migration: Create friend_requests table
        const string migration3 = """
            CREATE TABLE IF NOT EXISTS friend_requests (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                sender_id UUID REFERENCES users(id) ON DELETE CASCADE,
                receiver_id UUID REFERENCES users(id) ON DELETE CASCADE,
                status INTEGER NOT NULL DEFAULT 0,
                created_at TIMESTAMP DEFAULT NOW(),
                UNIQUE(sender_id, receiver_id)
            );
            """;
        await conn.ExecuteAsync(migration3);

        // Migration: Create friends table (bidirectional)
        const string migration4 = """
            CREATE TABLE IF NOT EXISTS friends (
                user_id UUID REFERENCES users(id) ON DELETE CASCADE,
                friend_id UUID REFERENCES users(id) ON DELETE CASCADE,
                created_at TIMESTAMP DEFAULT NOW(),
                PRIMARY KEY (user_id, friend_id)
            );
            """;
        await conn.ExecuteAsync(migration4);

        // Migration: Create indexes for friends
        const string migration5 = """
            CREATE INDEX IF NOT EXISTS idx_friends_user_id ON friends(user_id);
            CREATE INDEX IF NOT EXISTS idx_friends_friend_id ON friends(friend_id);
            CREATE INDEX IF NOT EXISTS idx_friend_requests_receiver ON friend_requests(receiver_id, status);
            CREATE INDEX IF NOT EXISTS idx_users_nickname ON users USING gin(nickname gin_trgm_ops);
            """;
        await conn.ExecuteAsync(migration5);

        // Migration: Create game_messages table (chat during game)
        const string migration6 = """
            CREATE TABLE IF NOT EXISTS game_messages (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                game_id UUID REFERENCES games(id) ON DELETE CASCADE,
                sender_id UUID REFERENCES users(id) ON DELETE CASCADE,
                text TEXT,
                media_url TEXT,
                media_type VARCHAR(20),
                created_at TIMESTAMP DEFAULT NOW()
            );
            """;
        await conn.ExecuteAsync(migration6);

        // Migration: Create chat_messages table (direct messages between friends)
        const string migration7 = """
            CREATE TABLE IF NOT EXISTS chat_messages (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                sender_id UUID REFERENCES users(id) ON DELETE CASCADE,
                receiver_id UUID REFERENCES users(id) ON DELETE CASCADE,
                text TEXT,
                media_url TEXT,
                media_type VARCHAR(20),
                is_read BOOLEAN DEFAULT FALSE,
                created_at TIMESTAMP DEFAULT NOW()
            );
            """;
        await conn.ExecuteAsync(migration7);

        // Migration: Create game_reactions table
        const string migration8 = """
            CREATE TABLE IF NOT EXISTS game_reactions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                game_id UUID REFERENCES games(id) ON DELETE CASCADE,
                player_id UUID REFERENCES users(id) ON DELETE CASCADE,
                reaction VARCHAR(50) NOT NULL,
                created_at TIMESTAMP DEFAULT NOW()
            );
            """;
        await conn.ExecuteAsync(migration8);

        // Migration: Create indexes for messages
        const string migration9 = """
            CREATE INDEX IF NOT EXISTS idx_game_messages_game_id ON game_messages(game_id, created_at);
            CREATE INDEX IF NOT EXISTS idx_chat_messages_sender ON chat_messages(sender_id, created_at);
            CREATE INDEX IF NOT EXISTS idx_chat_messages_receiver ON chat_messages(receiver_id, created_at);
            CREATE INDEX IF NOT EXISTS idx_chat_messages_conversation ON chat_messages(sender_id, receiver_id, created_at);
            CREATE INDEX IF NOT EXISTS idx_game_reactions_game ON game_reactions(game_id, created_at);
            """;
        await conn.ExecuteAsync(migration9);

        // Migration: Create player_stats table
        const string migration10 = """
            CREATE TABLE IF NOT EXISTS player_stats (
                user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
                games_played INTEGER DEFAULT 0,
                games_won INTEGER DEFAULT 0,
                games_lost INTEGER DEFAULT 0,
                total_score INTEGER DEFAULT 0,
                current_streak INTEGER DEFAULT 0,
                best_streak INTEGER DEFAULT 0,
                total_answers INTEGER DEFAULT 0,
                correct_answers INTEGER DEFAULT 0,
                last_played_at TIMESTAMP
            );
            """;
        await conn.ExecuteAsync(migration10);

        // Migration: Create achievements table
        const string migration11 = """
            CREATE TABLE IF NOT EXISTS achievements (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(100) NOT NULL,
                description VARCHAR(255),
                icon VARCHAR(50),
                points INTEGER DEFAULT 0,
                requirement_type VARCHAR(50),
                requirement_value INTEGER,
                created_at TIMESTAMP DEFAULT NOW()
            );
            """;
        await conn.ExecuteAsync(migration11);

        // Migration: Create user_achievements table
        const string migration12 = """
            CREATE TABLE IF NOT EXISTS user_achievements (
                user_id UUID REFERENCES users(id) ON DELETE CASCADE,
                achievement_id UUID REFERENCES achievements(id) ON DELETE CASCADE,
                earned_at TIMESTAMP DEFAULT NOW(),
                PRIMARY KEY (user_id, achievement_id)
            );
            """;
        await conn.ExecuteAsync(migration12);

        Console.WriteLine("Migraciones completadas.");
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
