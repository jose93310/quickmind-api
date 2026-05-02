using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMind.Application.DTOs;
using QuickMind.Application.Services;

namespace QuickMind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService) => _gameService = gameService;

    [HttpPost]
    public async Task<ActionResult<GameResponseDto>> CreateGame(CreateGameDto dto)
    {
        try
        {
            var game = await _gameService.CreateGameAsync(dto);
            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameResponseDto>> GetGame(Guid id)
    {
        var game = await _gameService.GetGameAsync(id);
        return Ok(game);
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<GameResponseDto>> GetGameByCode(string code)
    {
        var game = await _gameService.GetGameByCodeAsync(code);
        return Ok(game);
    }

    [HttpPost("join")]
    public async Task<ActionResult<GameResponseDto>> JoinGame(JoinGameDto dto)
    {
        try
        {
            var game = await _gameService.JoinGameAsync(dto.GameCode, dto.UserId);
            return Ok(game);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult> StartGame(Guid id, [FromBody] Guid hostId)
    {
        try
        {
            await _gameService.StartGameAsync(id, hostId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/stop")]
    public async Task<ActionResult> StopRound(Guid id, [FromBody] Guid playerId)
    {
        await _gameService.StopRoundAsync(id, playerId);
        return NoContent();
    }

    [HttpPost("answers")]
    public async Task<ActionResult> SubmitAnswer(AnswerSubmissionDto dto)
    {
        try
        {
            await _gameService.SubmitAnswerAsync(dto);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("vote")]
    public async Task<ActionResult> Vote(VoteDto dto, [FromQuery] Guid voterId)
    {
        await _gameService.VoteAnswerAsync(voterId, dto);
        return Ok();
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories([FromQuery] int? ageGroup)
    {
        var categories = await _gameService.GetCategoriesAsync(ageGroup);
        return Ok(categories);
    }
}
