using QuickMind.Application.DTOs;

namespace QuickMind.Application.Services;

public interface IGameService
{
    Task<GameResponseDto> CreateGameAsync(CreateGameDto dto);
    Task<GameResponseDto> JoinGameAsync(string code, Guid userId);
    Task StartGameAsync(Guid gameId, Guid hostId);
    Task StopRoundAsync(Guid gameId, Guid playerId);
    Task SubmitAnswerAsync(AnswerSubmissionDto dto);
    Task VoteAnswerAsync(Guid voterId, VoteDto dto);
    Task<GameResponseDto> GetGameAsync(Guid gameId);
    Task<GameResponseDto> GetGameByCodeAsync(string code);
    Task<List<PublicGameDto>> GetPublicGamesAsync();
    Task<List<CategoryDto>> GetCategoriesAsync(int? ageGroup = null);
    Task<GameResponseDto> UpdateGameSettingsAsync(Guid gameId, Guid hostId, CreateGameDto dto);
}
