using Microsoft.EntityFrameworkCore;
using TinyWondersAPI.DTOs.Categories;
using TinyWondersAPI.Models;
using TinyWondersAPI.Services.Interfaces;

namespace TinyWondersAPI.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Articles)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToResponse).ToList();
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id)
    {
        var category = await _context.Categories
            .Include(c => c.Articles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            throw new KeyNotFoundException("Category not found");

        return MapToResponse(category);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var slug = GenerateSlug(request.Name);

        var existing = await _context.Categories
            .AnyAsync(c => c.Slug == slug);

        if (existing)
            throw new InvalidOperationException("A category with this name already exists");

        var category = new Category
        {
            Name = request.Name,
            Slug = slug
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return MapToResponse(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            throw new KeyNotFoundException("Category not found");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Trim()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("&", "and")
            .Replace(",", "")
            .Replace(".", "");
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ArticleCount = category.Articles.Count(a => a.IsPublished),
            CreatedAt = category.CreatedAt
        };
    }
}