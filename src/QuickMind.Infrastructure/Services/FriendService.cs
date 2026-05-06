using QuickMind.Application.DTOs;
using QuickMind.Application.Services;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Hubs;

namespace QuickMind.Infrastructure.Services;

public class FriendService : IFriendService
{
    private readonly IFriendRepository _friendRepo;
    private readonly IFriendRequestRepository _requestRepo;
    private readonly IUserRepository _userRepo;
    private readonly IGameNotificationService _notifications;

    public FriendService(
        IFriendRepository friendRepo,
        IFriendRequestRepository requestRepo,
        IUserRepository userRepo,
        IGameNotificationService notifications)
    {
        _friendRepo = friendRepo;
        _requestRepo = requestRepo;
        _userRepo = userRepo;
        _notifications = notifications;
    }

    public async Task SendFriendRequestAsync(SendFriendRequestDto dto)
    {
        var sender = await _userRepo.GetByIdAsync(dto.SenderId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        var receiver = await _userRepo.GetByNicknameAsync(dto.ReceiverNickname)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        if (sender.Id == receiver.Id)
            throw new InvalidOperationException("No puedes enviarte una solicitud a ti mismo");

        var existingFriendship = await _friendRepo.AreFriendsAsync(sender.Id, receiver.Id);
        if (existingFriendship)
            throw new InvalidOperationException("Ya son amigos");

        var existingRequest = await _requestRepo.GetBetweenUsersAsync(sender.Id, receiver.Id);
        if (existingRequest != null && existingRequest.Status == FriendRequestStatus.Pending)
            throw new InvalidOperationException("Ya existe una solicitud pendiente");

        if (existingRequest != null && existingRequest.Status == FriendRequestStatus.Rejected)
        {
            existingRequest.Status = FriendRequestStatus.Pending;
            existingRequest.CreatedAt = DateTime.UtcNow;
            await _requestRepo.UpdateAsync(existingRequest);
        }
        else
        {
            var request = new FriendRequest
            {
                Id = Guid.NewGuid(),
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _requestRepo.CreateAsync(request);
            existingRequest = request;
        }

        var requestDto = new FriendRequestDto(
            existingRequest.Id,
            sender.Id,
            sender.Nickname,
            sender.AvatarPath,
            receiver.Id,
            (int)existingRequest.Status,
            existingRequest.CreatedAt
        );

        await _notifications.NotifyFriendRequestReceived(receiver.Id, requestDto);
    }

    public async Task RespondToRequestAsync(RespondFriendRequestDto dto)
    {
        var request = await _requestRepo.GetByIdAsync(dto.RequestId)
            ?? throw new KeyNotFoundException("Solicitud no encontrada");

        if (request.ReceiverId != dto.UserId)
            throw new UnauthorizedAccessException("No tienes permiso para responder esta solicitud");

        if (request.Status != FriendRequestStatus.Pending)
            throw new InvalidOperationException("Esta solicitud ya fue respondida");

        if (dto.Accept)
        {
            request.Status = FriendRequestStatus.Accepted;
            await _requestRepo.UpdateAsync(request);
            await _friendRepo.AddFriendAsync(request.SenderId, request.ReceiverId);
        }
        else
        {
            request.Status = FriendRequestStatus.Rejected;
            await _requestRepo.UpdateAsync(request);
        }

        await _notifications.NotifyFriendRequestResponded(request.SenderId, dto.RequestId, dto.Accept);
    }

    public async Task<List<FriendRequestDto>> GetPendingRequestsAsync(Guid userId)
    {
        var requests = await _requestRepo.GetPendingByReceiverAsync(userId);
        var result = new List<FriendRequestDto>();

        foreach (var req in requests)
        {
            var sender = await _userRepo.GetByIdAsync(req.SenderId);
            result.Add(new FriendRequestDto(
                req.Id,
                req.SenderId,
                sender?.Nickname ?? "Desconocido",
                sender?.AvatarPath,
                req.ReceiverId,
                (int)req.Status,
                req.CreatedAt
            ));
        }

        return result;
    }

    public async Task<List<FriendDto>> GetFriendsAsync(Guid userId)
    {
        var friends = await _friendRepo.GetFriendsAsync(userId);
        return friends.Select(f => new FriendDto(
            f.Id,
            f.Nickname,
            f.AvatarPath,
            false // TODO: implement online status tracking
        )).ToList();
    }

    public async Task RemoveFriendAsync(Guid userId, Guid friendId)
    {
        var areFriends = await _friendRepo.AreFriendsAsync(userId, friendId);
        if (!areFriends)
            throw new KeyNotFoundException("No son amigos");

        await _friendRepo.RemoveFriendAsync(userId, friendId);
    }

    public async Task<List<UserSearchDto>> SearchUsersAsync(Guid userId, string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<UserSearchDto>();

        var users = await _userRepo.SearchByNicknameAsync(query, userId);
        var result = new List<UserSearchDto>();

        foreach (var user in users)
        {
            var isFriend = await _friendRepo.AreFriendsAsync(userId, user.Id);
            var existingRequest = await _requestRepo.GetBetweenUsersAsync(userId, user.Id);
            var hasPendingRequest = existingRequest?.Status == FriendRequestStatus.Pending;

            result.Add(new UserSearchDto(
                user.Id,
                user.Nickname,
                user.AvatarPath,
                isFriend,
                hasPendingRequest
            ));
        }

        return result;
    }
}
