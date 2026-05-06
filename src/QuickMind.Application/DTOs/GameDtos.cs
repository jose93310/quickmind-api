namespace QuickMind.Application.DTOs;

public record CreateGameDto(
    Guid HostId,
    int MaxPlayers,
    int TotalRounds,
    int TimePerRound,
    int LetterMode,
    int ValidationType,
    List<int> CategoryIds,
    bool IsPublic = false,
    DateTime? ScheduledStart = null
);

public record JoinGameDto(
    string GameCode,
    Guid UserId
);

public record GameResponseDto(
    Guid Id,
    string Code,
    Guid HostId,
    int Status,
    int CurrentRound,
    int TotalRounds,
    int TimePerRound,
    string? CurrentLetter,
    List<PlayerInfoDto> Players,
    bool IsPublic = false,
    DateTime? ScheduledStart = null
);

public record PlayerInfoDto(
    Guid Id,
    Guid UserId,
    string Nickname,
    string? AvatarPath,
    int Score,
    bool IsHost,
    bool IsOnline
);

public record AnswerSubmissionDto(
    Guid RoundId,
    Guid PlayerId,
    string Category,
    string Text
);

public record VoteDto(
    Guid AnswerId,
    bool IsValid
);

public record CategoryDto(
    int Id,
    string Name,
    string DisplayName,
    int AgeGroup,
    string? Icon
);

public record PublicGameDto(
    Guid Id,
    string Code,
    string HostNickname,
    int Status,
    int CurrentPlayers,
    int MaxPlayers,
    int TotalRounds,
    int TimePerRound,
    DateTime? ScheduledStart,
    int MinutesUntilStart
);
