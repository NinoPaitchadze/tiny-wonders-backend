using Microsoft.EntityFrameworkCore;
using TinyWondersAPI.DTOs.Comments;
using TinyWondersAPI.Models;
using TinyWondersAPI.Services.Interfaces;

namespace TinyWondersAPI.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;

    public CommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommentResponse>> GetAllForArticleAsync(Guid articleId)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new KeyNotFoundException("Article not found");

        var comments = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.ArticleId == articleId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(MapToResponse).ToList();
    }

    public async Task<CommentResponse> CreateAsync(Guid articleId, CreateCommentRequest request, Guid userId)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId && a.IsPublished);

        if (article == null)
            throw new KeyNotFoundException("Article not found");

        var comment = new Comment
        {
            ArticleId = articleId,
            UserId = userId,
            Content = request.Content
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        await _context.Entry(comment).Reference(c => c.User).LoadAsync();

        return MapToResponse(comment);
    }

    public async Task DeleteAsync(Guid commentId, Guid userId, bool isAdmin)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new KeyNotFoundException("Comment not found");

        if (!isAdmin && comment.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own comments");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    private static CommentResponse MapToResponse(Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            ArticleId = comment.ArticleId,
            UserId = comment.UserId,
            UserName = comment.User.FullName,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}

