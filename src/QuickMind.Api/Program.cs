using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuickMind.Application.Services;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Auth;
using QuickMind.Infrastructure.Data;
using QuickMind.Infrastructure.Hubs;
using QuickMind.Infrastructure.Repositories;
using QuickMind.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi("v1");

// CORS para Flutter
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// SignalR para tiempo real
builder.Services.AddSignalR();

// PostgreSQL + Dapper - Lee de variables de entorno o appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new NpgsqlConnectionFactory(connectionString));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Servicios de Auth
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Repositorios de juego
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameRoundRepository, GameRoundRepository>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IRoundVoteRepository, RoundVoteRepository>();

// Repositorios de amigos
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();

// Servicios de juego
builder.Services.AddScoped<IGameNotificationService, GameNotificationService>();
builder.Services.AddScoped<IGameService, GameService>();

// Repositorios de chat
builder.Services.AddScoped<IGameMessageRepository, GameMessageRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// Servicios de amigos
builder.Services.AddScoped<IFriendService, FriendService>();

// Servicios de chat
builder.Services.AddScoped<IChatService, ChatService>();

// Repositorios de stats
builder.Services.AddScoped<IStatsRepository, StatsRepository>();
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();

// Servicios de stats
builder.Services.AddScoped<IStatsService, StatsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFlutter");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

// Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Inicializar base de datos
try
{
    using var scope = app.Services.CreateScope();
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    await DatabaseInitializer.InitializeAsync(dbFactory);
}
catch (Exception ex)
{
    Console.WriteLine($"Inicialización de BD pendiente: {ex.Message}");
}

app.Run();
