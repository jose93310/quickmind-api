using QuickMind.Application.DTOs;
using QuickMind.Application.Services;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Hubs;

namespace QuickMind.Infrastructure.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepo;
    private readonly IPlayerRepository _playerRepo;
    private readonly IGameRoundRepository _roundRepo;
    private readonly IAnswerRepository _answerRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IUserRepository _userRepo;
    private readonly IRoundVoteRepository _voteRepo;
    private readonly IGameNotificationService _notifications;

    public GameService(
        IGameRepository gameRepo,
        IPlayerRepository playerRepo,
        IGameRoundRepository roundRepo,
        IAnswerRepository answerRepo,
        ICategoryRepository categoryRepo,
        IUserRepository userRepo,
        IRoundVoteRepository voteRepo,
        IGameNotificationService notifications)
    {
        _gameRepo = gameRepo;
        _playerRepo = playerRepo;
        _roundRepo = roundRepo;
        _answerRepo = answerRepo;
        _categoryRepo = categoryRepo;
        _userRepo = userRepo;
        _voteRepo = voteRepo;
        _notifications = notifications;
    }

    public async Task<GameResponseDto> CreateGameAsync(CreateGameDto dto)
    {
        if (dto.MaxPlayers < 2 || dto.MaxPlayers > 8)
            throw new InvalidOperationException("Mínimo 2 jugadores, máximo 8");

        if (dto.TotalRounds < 1 || dto.TotalRounds > 100)
            throw new InvalidOperationException("Mínimo 1 ronda, máximo 100");

        if (dto.TimePerRound < 30 || dto.TimePerRound > 600)
            throw new InvalidOperationException("Tiempo entre 30s y 600s (10 min)");

        var user = await _userRepo.GetByIdAsync(dto.HostId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Code = GenerateCode(),
            HostId = dto.HostId,
            Status = GameStatus.Waiting,
            MaxPlayers = dto.MaxPlayers,
            TotalRounds = dto.TotalRounds,
            CurrentRound = 0,
            TimePerRound = dto.TimePerRound,
            LetterMode = (LetterMode)dto.LetterMode,
            ValidationType = (ValidationType)dto.ValidationType,
            CreatedAt = DateTime.UtcNow
        };

        await _gameRepo.CreateAsync(game);

        var hostPlayer = new Player
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            UserId = user.Id,
            Nickname = user.Nickname,
            AvatarPath = user.AvatarPath,
            IsHost = true,
            IsOnline = true,
            JoinedAt = DateTime.UtcNow
        };

        await _playerRepo.AddAsync(hostPlayer);

        return MapToResponse(game, new[] { hostPlayer });
    }

    public async Task<GameResponseDto> JoinGameAsync(string code, Guid userId)
    {
        var game = await _gameRepo.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException("Partida no encontrada");

        if (game.Status != GameStatus.Waiting)
            throw new InvalidOperationException("La partida ya no acepta jugadores");

        var count = await _playerRepo.GetPlayerCountAsync(game.Id);
        if (count >= game.MaxPlayers)
            throw new InvalidOperationException("La partida está llena");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        var player = new Player
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            UserId = user.Id,
            Nickname = user.Nickname,
            AvatarPath = user.AvatarPath,
            IsHost = false,
            IsOnline = true,
            JoinedAt = DateTime.UtcNow
        };

        await _playerRepo.AddAsync(player);

        var players = await _playerRepo.GetByGameIdAsync(game.Id);

        var playerDto = new PlayerInfoDto(player.Id, player.UserId, player.Nickname,
            player.AvatarPath, player.Score, player.IsHost, player.IsOnline);

        await _notifications.NotifyPlayerJoined(game.Code, playerDto);

        return MapToResponse(game, players);
    }

    public async Task StartGameAsync(Guid gameId, Guid hostId)
    {
        var game = await _gameRepo.GetByIdAsync(gameId)
            ?? throw new KeyNotFoundException("Partida no encontrada");

        if (game.HostId != hostId)
            throw new UnauthorizedAccessException("Solo el host puede iniciar la partida");

        var playerCount = await _playerRepo.GetPlayerCountAsync(gameId);
        if (playerCount < 2)
            throw new InvalidOperationException("Se necesitan al menos 2 jugadores");

        var letter = GenerateLetter();
        game.Status = GameStatus.InProgress;
        game.CurrentRound = 1;
        game.StartedAt = DateTime.UtcNow;
        await _gameRepo.UpdateAsync(game);

        var round = new GameRound
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            RoundNumber = 1,
            Letter = letter,
            StartedAt = DateTime.UtcNow,
            Status = RoundStatus.InProgress
        };
        await _roundRepo.CreateAsync(round);

        await _notifications.NotifyGameStarted(game.Code, letter, 1, game.TotalRounds);
    }

    public async Task StopRoundAsync(Guid gameId, Guid playerId)
    {
        var game = await _gameRepo.GetByIdAsync(gameId)
            ?? throw new KeyNotFoundException("Partida no encontrada");

        var rounds = await _roundRepo.GetByGameIdAsync(gameId);
        var currentRound = rounds.LastOrDefault(r => r.Status == RoundStatus.InProgress)
            ?? throw new InvalidOperationException("No hay ronda en curso");

        currentRound.Status = RoundStatus.Finished;
        currentRound.EndedAt = DateTime.UtcNow;
        await _roundRepo.UpdateAsync(currentRound);

        await _notifications.NotifyRoundStopped(game.Code, playerId);
    }

    public async Task SubmitAnswerAsync(AnswerSubmissionDto dto)
    {
        var alreadyAnswered = await _answerRepo.HasPlayerAnsweredAsync(
            dto.RoundId, dto.PlayerId, dto.Category);

        if (alreadyAnswered)
            throw new InvalidOperationException("Ya enviaste respuesta para esta categoría");

        var answer = new Answer
        {
            Id = Guid.NewGuid(),
            RoundId = dto.RoundId,
            PlayerId = dto.PlayerId,
            Category = dto.Category,
            Text = dto.Text,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _answerRepo.CreateAsync(answer);
    }

    public async Task VoteAnswerAsync(Guid voterId, VoteDto dto)
    {
        var answer = await _answerRepo.GetByIdAsync(dto.AnswerId)
            ?? throw new KeyNotFoundException("Respuesta no encontrada");

        var vote = new RoundVote
        {
            Id = Guid.NewGuid(),
            AnswerId = dto.AnswerId,
            VoterId = voterId,
            IsValid = dto.IsValid,
            CreatedAt = DateTime.UtcNow
        };

        await _voteRepo.CreateAsync(vote);
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(int? ageGroup = null)
    {
        IEnumerable<Category> categories;

        if (ageGroup.HasValue)
            categories = await _categoryRepo.GetByAgeGroupAsync((AgeGroup)ageGroup.Value);
        else
            categories = await _categoryRepo.GetAllAsync();

        return categories.Select(c => new CategoryDto(
            c.Id, c.Name, c.DisplayName, (int)c.AgeGroup, c.Icon)).ToList();
    }

    public async Task<GameResponseDto> GetGameAsync(Guid gameId)
    {
        var game = await _gameRepo.GetByIdAsync(gameId)
            ?? throw new KeyNotFoundException("Partida no encontrada");

        var players = await _playerRepo.GetByGameIdAsync(gameId);
        return MapToResponse(game, players);
    }

    public async Task<GameResponseDto> GetGameByCodeAsync(string code)
    {
        var game = await _gameRepo.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException("Partida no encontrada");

        var players = await _playerRepo.GetByGameIdAsync(game.Id);
        return MapToResponse(game, players);
    }

    private string GenerateCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }

    private string GenerateLetter()
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return letters[Random.Shared.Next(letters.Length)].ToString();
    }

    private static GameResponseDto MapToResponse(Game game, IEnumerable<Player> players)
    {
        return new GameResponseDto(
            game.Id, game.Code, game.HostId, (int)game.Status,
            game.CurrentRound, game.TotalRounds, game.TimePerRound,
            null,
            players.Select(p => new PlayerInfoDto(
                p.Id, p.UserId, p.Nickname, p.AvatarPath, p.Score, p.IsHost, p.IsOnline)).ToList());
    }
}
