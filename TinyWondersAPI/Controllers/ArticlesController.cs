using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TinyWondersAPI.DTOs.Articles;
using TinyWondersAPI.Services.Interfaces;

namespace TinyWondersAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly IImageService _imageService;

    public ArticlesController(IArticleService articleService, IImageService imageService)
    {
        _articleService = articleService;
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId = null)
    {
        var articles = await _articleService.GetAllPublishedAsync(categoryId);
        return Ok(articles);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAdmin()
    {
        var articles = await _articleService.GetAllAsync();
        return Ok(articles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var isAdmin = User.IsInRole("Admin");
            var article = await _articleService.GetByIdAsync(id, isAdmin);
            return Ok(article);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateArticleRequest request)
    {
        try
        {
            var authorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var article = await _articleService.CreateAsync(request, authorId);
            return CreatedAtAction(nameof(GetById), new { id = article.Id }, article);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleRequest request)
    {
        try
        {
            var article = await _articleService.UpdateAsync(id, request);
            return Ok(article);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/image")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateImage(Guid id, IFormFile file)
    {
        try
        {
            var existing = await _articleService.GetByIdAsync(id, true);
            if (!string.IsNullOrEmpty(existing.CoverImageUrl))
                await _imageService.DeleteImageAsync(existing.CoverImageUrl);

            var imageUrl = await _imageService.UploadImageAsync(file);
            var article = await _articleService.UpdateImageAsync(id, imageUrl);
            return Ok(article);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _articleService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
