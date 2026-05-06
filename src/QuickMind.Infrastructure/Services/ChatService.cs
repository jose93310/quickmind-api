using QuickMind.Application.DTOs;
using QuickMind.Application.Services;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Hubs;

namespace QuickMind.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly IGameMessageRepository _gameMessageRepo;
    private readonly IChatMessageRepository _chatMessageRepo;
    private readonly IUserRepository _userRepo;
    private readonly IGameNotificationService _notifications;

    public ChatService(
        IGameMessageRepository gameMessageRepo,
        IChatMessageRepository chatMessageRepo,
        IUserRepository userRepo,
        IGameNotificationService notifications)
    {
        _gameMessageRepo = gameMessageRepo;
        _chatMessageRepo = chatMessageRepo;
        _userRepo = userRepo;
        _notifications = notifications;
    }

    public async Task<GameMessageDto> SendGameMessageAsync(SendGameMessageDto dto)
    {
        var sender = await _userRepo.GetByIdAsync(dto.SenderId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        var message = new GameMessage
        {
            Id = Guid.NewGuid(),
            GameId = dto.GameId,
            SenderId = dto.SenderId,
            Text = dto.Text,
            MediaUrl = dto.MediaUrl,
            MediaType = dto.MediaType,
            CreatedAt = DateTime.UtcNow
        };

        await _gameMessageRepo.CreateAsync(message);

        var result = new GameMessageDto(
            message.Id,
            message.GameId,
            message.SenderId,
            sender.Nickname,
            message.Text,
            message.MediaUrl,
            message.MediaType,
            message.CreatedAt
        );

        await _notifications.NotifyGameMessageReceived(message.GameId, result);
        return result;
    }

    public async Task<List<GameMessageDto>> GetGameMessagesAsync(Guid gameId)
    {
        var messages = await _gameMessageRepo.GetByGameIdAsync(gameId);
        var result = new List<GameMessageDto>();

        foreach (var msg in messages)
        {
            var sender = await _userRepo.GetByIdAsync(msg.SenderId);
            result.Add(new GameMessageDto(
                msg.Id,
                msg.GameId,
                msg.SenderId,
                sender?.Nickname ?? "Desconocido",
                msg.Text,
                msg.MediaUrl,
                msg.MediaType,
                msg.CreatedAt
            ));
        }

        result.Reverse();
        return result;
    }

    public async Task<ChatMessageDto> SendChatMessageAsync(SendChatMessageDto dto)
    {
        var sender = await _userRepo.GetByIdAsync(dto.SenderId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SenderId = dto.SenderId,
            ReceiverId = dto.ReceiverId,
            Text = dto.Text,
            MediaUrl = dto.MediaUrl,
            MediaType = dto.MediaType,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _chatMessageRepo.CreateAsync(message);

        var result = new ChatMessageDto(
            message.Id,
            message.SenderId,
            sender.Nickname,
            message.ReceiverId,
            message.Text,
            message.MediaUrl,
            message.MediaType,
            message.IsRead,
            message.CreatedAt
        );

        await _notifications.NotifyChatMessageReceived(message.ReceiverId, result);
        return result;
    }

    public async Task<List<ChatMessageDto>> GetConversationAsync(Guid userId1, Guid userId2)
    {
        var messages = await _chatMessageRepo.GetConversationAsync(userId1, userId2);
        var result = new List<ChatMessageDto>();

        foreach (var msg in messages)
        {
            var sender = await _userRepo.GetByIdAsync(msg.SenderId);
            result.Add(new ChatMessageDto(
                msg.Id,
                msg.SenderId,
                sender?.Nickname ?? "Desconocido",
                msg.ReceiverId,
                msg.Text,
                msg.MediaUrl,
                msg.MediaType,
                msg.IsRead,
                msg.CreatedAt
            ));
        }

        result.Reverse();
        return result;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _chatMessageRepo.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(Guid messageId)
    {
        await _chatMessageRepo.MarkAsReadAsync(messageId);
    }

    public async Task<GameReactionDto> SendReactionAsync(SendReactionDto dto)
    {
        var player = await _userRepo.GetByIdAsync(dto.PlayerId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        var reaction = new GameReaction
        {
            Id = Guid.NewGuid(),
            GameId = dto.GameId,
            PlayerId = dto.PlayerId,
            Reaction = dto.Reaction,
            CreatedAt = DateTime.UtcNow
        };

        // Solo notificar, no persistir reacciones (son efímeras)
        var result = new GameReactionDto(
            reaction.Id,
            reaction.GameId,
            reaction.PlayerId,
            player.Nickname,
            reaction.Reaction,
            reaction.CreatedAt
        );

        await _notifications.NotifyGameReaction(dto.GameId, result);
        return result;
    }

    public async Task CleanupOldMessagesAsync()
    {
        await _gameMessageRepo.DeleteOldMessagesAsync(TimeSpan.FromHours(24));
        await _chatMessageRepo.DeleteOldMessagesAsync(TimeSpan.FromHours(24));
    }
}
