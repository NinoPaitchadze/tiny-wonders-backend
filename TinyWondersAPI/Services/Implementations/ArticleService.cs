using Microsoft.EntityFrameworkCore;
using TinyWondersAPI.DTOs.Articles;
using TinyWondersAPI.Models;
using TinyWondersAPI.Services.Interfaces;

namespace TinyWondersAPI.Services.Implementations;

public class ArticleService : IArticleService
{
    private readonly ApplicationDbContext _context;

    public ArticleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArticleResponse>> GetAllPublishedAsync(Guid? categoryId = null)
    {
        var query = _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .Where(a => a.IsPublished);

        if (categoryId.HasValue)
            query = query.Where(a => a.CategoryId == categoryId);

        var articles = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return articles.Select(MapToResponse).ToList();
    }

    public async Task<List<ArticleResponse>> GetAllAsync()
    {
        var articles = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return articles.Select(MapToResponse).ToList();
    }

    public async Task<ArticleResponse> GetByIdAsync(Guid id, bool isAdmin = false)
    {
        var article = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null)
            throw new KeyNotFoundException("Article not found");

        if (!isAdmin && !article.IsPublished)
            throw new KeyNotFoundException("Article not found");

        return MapToResponse(article);
    }

    public async Task<ArticleResponse> CreateAsync(CreateArticleRequest request, Guid authorId)
    {
        var article = new Article
        {
            Title = request.Title,
            Content = request.Content,
            CategoryId = request.CategoryId,
            IsPublished = request.IsPublished,
            AuthorId = authorId
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        await _context.Entry(article).Reference(a => a.Author).LoadAsync();
        if (article.CategoryId.HasValue)
            await _context.Entry(article).Reference(a => a.Category).LoadAsync();

        return MapToResponse(article);
    }

    public async Task<ArticleResponse> UpdateAsync(Guid id, UpdateArticleRequest request)
    {
        var article = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null)
            throw new KeyNotFoundException("Article not found");

        article.Title = request.Title;
        article.Content = request.Content;
        article.CategoryId = request.CategoryId;
        article.IsPublished = request.IsPublished;
        article.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (article.CategoryId.HasValue)
            await _context.Entry(article).Reference(a => a.Category).LoadAsync();

        return MapToResponse(article);
    }

    public async Task<ArticleResponse> UpdateImageAsync(Guid id, string imageUrl)
    {
        var article = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null)
            throw new KeyNotFoundException("Article not found");

        article.CoverImageUrl = imageUrl;
        article.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToResponse(article);
    }

    public async Task DeleteAsync(Guid id)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null)
            throw new KeyNotFoundException("Article not found");

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();
    }

    private static ArticleResponse MapToResponse(Article article)
    {
        return new ArticleResponse
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            CoverImageUrl = article.CoverImageUrl,
            IsPublished = article.IsPublished,
            AuthorId = article.AuthorId,
            AuthorName = article.Author.FullName,
            CategoryId = article.CategoryId,
            CategoryName = article.Category?.Name,
            CommentCount = article.Comments.Count,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        };
    }
}
