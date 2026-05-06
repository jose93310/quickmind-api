using Microsoft.AspNetCore.Mvc;
using QuickMind.Application.DTOs;
using QuickMind.Application.Services;

namespace QuickMind.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("game")]
    public async Task<ActionResult<GameMessageDto>> SendGameMessage(SendGameMessageDto dto)
    {
        try
        {
            var message = await _chatService.SendGameMessageAsync(dto);
            return Ok(message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<List<GameMessageDto>>> GetGameMessages(Guid gameId)
    {
        var messages = await _chatService.GetGameMessagesAsync(gameId);
        return Ok(messages);
    }

    [HttpPost("direct")]
    public async Task<ActionResult<ChatMessageDto>> SendChatMessage(SendChatMessageDto dto)
    {
        try
        {
            var message = await _chatService.SendChatMessageAsync(dto);
            return Ok(message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("direct")]
    public async Task<ActionResult<List<ChatMessageDto>>> GetConversation(
        [FromQuery] Guid userId1,
        [FromQuery] Guid userId2)
    {
        var messages = await _chatService.GetConversationAsync(userId1, userId2);
        return Ok(messages);
    }

    [HttpGet("unread/{userId}")]
    public async Task<ActionResult<int>> GetUnreadCount(Guid userId)
    {
        var count = await _chatService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    [HttpPost("read/{messageId}")]
    public async Task<ActionResult> MarkAsRead(Guid messageId)
    {
        await _chatService.MarkAsReadAsync(messageId);
        return Ok(new { message = "Mensaje marcado como leído" });
    }

    [HttpPost("reaction")]
    public async Task<ActionResult<GameReactionDto>> SendReaction(SendReactionDto dto)
    {
        try
        {
            var reaction = await _chatService.SendReactionAsync(dto);
            return Ok(reaction);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
