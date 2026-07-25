using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TinyWondersAPI.DTOs.Comments;
using TinyWondersAPI.Hubs;
using TinyWondersAPI.Services.Interfaces;

namespace TinyWondersAPI.Controllers;

[ApiController]
[Route("api/articles/{articleId}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IHubContext<CommentHub> _hubContext;

    public CommentsController(ICommentService commentService, IHubContext<CommentHub> hubContext)
    {
        _commentService = commentService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid articleId)
    {
        try
        {
            var comments = await _commentService.GetAllForArticleAsync(articleId);
            return Ok(comments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Guid articleId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var comment = await _commentService.CreateAsync(articleId, request, userId);

            // Broadcast the new comment to everyone viewing this article
            await _hubContext.Clients
                .Group(articleId.ToString())
                .SendAsync("NewComment", comment);

            return Ok(comment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid articleId, Guid commentId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");
            await _commentService.DeleteAsync(commentId, userId, isAdmin);

            // Broadcast the deletion to everyone viewing this article
            await _hubContext.Clients
                .Group(articleId.ToString())
                .SendAsync("DeleteComment", commentId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}