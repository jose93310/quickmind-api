using Microsoft.AspNetCore.SignalR;
using QuickMind.Application.DTOs;

namespace QuickMind.Infrastructure.Hubs;

public class GameHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

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
    Task NotifyFriendRequestReceived(Guid receiverId, FriendRequestDto request);
    Task NotifyFriendRequestResponded(Guid senderId, Guid requestId, bool accepted);
    Task NotifyGameInviteReceived(Guid invitedUserId, GameInviteDto invite);
    Task NotifyUserOnlineStatus(Guid userId, bool isOnline);
    Task NotifyGameMessageReceived(Guid gameId, GameMessageDto message);
    Task NotifyChatMessageReceived(Guid receiverId, ChatMessageDto message);
    Task NotifyGameReaction(Guid gameId, GameReactionDto reaction);
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

    public async Task NotifyFriendRequestReceived(Guid receiverId, FriendRequestDto request)
        => await _hubContext.Clients.Group($"user_{receiverId}").SendAsync("FriendRequestReceived", request);

    public async Task NotifyFriendRequestResponded(Guid senderId, Guid requestId, bool accepted)
        => await _hubContext.Clients.Group($"user_{senderId}").SendAsync("FriendRequestResponded", requestId, accepted);

    public async Task NotifyGameInviteReceived(Guid invitedUserId, GameInviteDto invite)
        => await _hubContext.Clients.Group($"user_{invitedUserId}").SendAsync("GameInviteReceived", invite);

    public async Task NotifyUserOnlineStatus(Guid userId, bool isOnline)
        => await _hubContext.Clients.All.SendAsync("UserOnlineStatusChanged", userId, isOnline);

    public async Task NotifyGameMessageReceived(Guid gameId, GameMessageDto message)
        => await _hubContext.Clients.Group(gameId.ToString()).SendAsync("GameMessageReceived", message);

    public async Task NotifyChatMessageReceived(Guid receiverId, ChatMessageDto message)
        => await _hubContext.Clients.Group($"user_{receiverId}").SendAsync("ChatMessageReceived", message);

    public async Task NotifyGameReaction(Guid gameId, GameReactionDto reaction)
        => await _hubContext.Clients.Group(gameId.ToString()).SendAsync("GameReactionReceived", reaction);
}
