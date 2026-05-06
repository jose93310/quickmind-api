using Microsoft.AspNetCore.Mvc;
using QuickMind.Application.DTOs;
using QuickMind.Application.Services;

namespace QuickMind.Api.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet("player/{userId}")]
    public async Task<ActionResult<PlayerStatsDto>> GetPlayerStats(Guid userId)
    {
        try
        {
            var stats = await _statsService.GetPlayerStatsAsync(userId);
            return Ok(stats);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("achievements/{userId}")]
    public async Task<ActionResult<List<AchievementDto>>> GetAchievements(Guid userId)
    {
        var achievements = await _statsService.GetAchievementsAsync(userId);
        return Ok(achievements);
    }

    [HttpGet("categories/{userId}")]
    public async Task<ActionResult<List<CategoryStatsDto>>> GetCategoryStats(Guid userId)
    {
        var stats = await _statsService.GetCategoryStatsAsync(userId);
        return Ok(stats);
    }

    [HttpGet("history/{userId}")]
    public async Task<ActionResult<List<RoundHistoryDto>>> GetRoundHistory(Guid userId, [FromQuery] int limit = 20)
    {
        var history = await _statsService.GetRoundHistoryAsync(userId, limit);
        return Ok(history);
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<LeaderboardEntryDto>>> GetLeaderboard([FromQuery] int limit = 10)
    {
        var leaderboard = await _statsService.GetLeaderboardAsync(limit);
        return Ok(leaderboard);
    }

    [HttpPost("update")]
    public async Task<ActionResult> UpdateStats([FromQuery] Guid userId, [FromQuery] int score, [FromQuery] bool won)
    {
        await _statsService.UpdateStatsAfterGameAsync(userId, score, won);
        return Ok(new { message = "Estadísticas actualizadas" });
    }
}