using Microsoft.AspNetCore.SignalR;
using QuickMind.Application.DTOs;

namespace QuickMind.Infrastructure.Hubs;

public class GameHub : Hub
{
    public async Task JoinGame(string gameCode, PlayerInfoDto player)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        await Clients.OthersInGroup(gameCode).SendAsync("PlayerJoined", player);
    }

    public async Task LeaveGame(string gameCode, Guid playerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameCode);
        await Clients.OthersInGroup(gameCode).SendAsync("PlayerLeft", playerId);
    }

    public async Task StartGame(string gameCode, string letter, int currentRound, int totalRounds)
    {
        await Clients.Group(gameCode).SendAsync("GameStarted", letter, currentRound, totalRounds);
    }

    public async Task StopRound(string gameCode, Guid playerId)
    {
        await Clients.Group(gameCode).SendAsync("RoundStopped", playerId);
    }

    public async Task AnswerSubmitted(string gameCode, Guid playerId, string category)
    {
        await Clients.OthersInGroup(gameCode).SendAsync("PlayerAnswered", playerId, category);
    }

    public async Task RoundResultsReady(string gameCode)
    {
        await Clients.Group(gameCode).SendAsync("RoundResultsReady");
    }

    public async Task GameFinished(string gameCode, List<PlayerInfoDto> finalScores)
    {
        await Clients.Group(gameCode).SendAsync("GameFinished", finalScores);
    }
}

public interface IGameNotificationService
{
    Task NotifyPlayerJoined(string gameCode, PlayerInfoDto player);
    Task NotifyPlayerLeft(string gameCode, Guid playerId);
    Task NotifyGameStarted(string gameCode, string letter, int currentRound, int totalRounds);
    Task NotifyRoundStopped(string gameCode, Guid playerId);
    Task NotifyAnswerSubmitted(string gameCode, Guid playerId, string category);
    Task NotifyRoundResultsReady(string gameCode);
    Task NotifyGameFinished(string gameCode, List<PlayerInfoDto> finalScores);
}

public class GameNotificationService : IGameNotificationService
{
    private readonly IHubContext<GameHub> _hubContext;

    public GameNotificationService(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyPlayerJoined(string gameCode, PlayerInfoDto player)
        => await _hubContext.Clients.Group(gameCode).SendAsync("PlayerJoined", player);

    public async Task NotifyPlayerLeft(string gameCode, Guid playerId)
        => await _hubContext.Clients.Group(gameCode).SendAsync("PlayerLeft", playerId);

    public async Task NotifyGameStarted(string gameCode, string letter, int currentRound, int totalRounds)
        => await _hubContext.Clients.Group(gameCode).SendAsync("GameStarted", letter, currentRound, totalRounds);

    public async Task NotifyRoundStopped(string gameCode, Guid playerId)
        => await _hubContext.Clients.Group(gameCode).SendAsync("RoundStopped", playerId);

    public async Task NotifyAnswerSubmitted(string gameCode, Guid playerId, string category)
        => await _hubContext.Clients.Group(gameCode).SendAsync("PlayerAnswered", playerId, category);

    public async Task NotifyRoundResultsReady(string gameCode)
        => await _hubContext.Clients.Group(gameCode).SendAsync("RoundResultsReady");

    public async Task NotifyGameFinished(string gameCode, List<PlayerInfoDto> finalScores)
        => await _hubContext.Clients.Group(gameCode).SendAsync("GameFinished", finalScores);
}
