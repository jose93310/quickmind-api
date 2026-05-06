using Microsoft.AspNetCore.Mvc;
using QuickMind.Application.DTOs;
using QuickMind.Application.Services;

namespace QuickMind.Api.Controllers;

[ApiController]
[Route("api/friends")]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    [HttpPost("request")]
    public async Task<ActionResult> SendRequest(SendFriendRequestDto dto)
    {
        try
        {
            await _friendService.SendFriendRequestAsync(dto);
            return Ok(new { message = "Solicitud enviada correctamente" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("respond")]
    public async Task<ActionResult> RespondToRequest(RespondFriendRequestDto dto)
    {
        try
        {
            await _friendService.RespondToRequestAsync(dto);
            return Ok(new { message = "Respuesta registrada correctamente" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("requests")]
    public async Task<ActionResult<List<FriendRequestDto>>> GetPendingRequests([FromQuery] Guid userId)
    {
        var requests = await _friendService.GetPendingRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpGet]
    public async Task<ActionResult<List<FriendDto>>> GetFriends([FromQuery] Guid userId)
    {
        var friends = await _friendService.GetFriendsAsync(userId);
        return Ok(friends);
    }

    [HttpDelete("{friendId}")]
    public async Task<ActionResult> RemoveFriend([FromQuery] Guid userId, Guid friendId)
    {
        try
        {
            await _friendService.RemoveFriendAsync(userId, friendId);
            return Ok(new { message = "Amigo eliminado correctamente" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<UserSearchDto>>> SearchUsers([FromQuery] Guid userId, [FromQuery] string query)
    {
        var users = await _friendService.SearchUsersAsync(userId, query);
        return Ok(users);
    }
}
