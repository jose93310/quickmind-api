namespace QuickMind.Application.DTOs;

public record CreateGameDto(
    Guid HostId,
    int MaxPlayers,
    int TotalRounds,
    int TimePerRound,
    int LetterMode,
    int ValidationType,
    List<int> CategoryIds
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
    List<PlayerInfoDto> Players
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
