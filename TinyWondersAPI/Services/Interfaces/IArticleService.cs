using TinyWondersAPI.DTOs.Articles;

namespace TinyWondersAPI.Services.Interfaces
{
    public interface IArticleService
    {
        Task<List<ArticleResponse>> GetAllPublishedAsync(Guid? categoryId = null);//for regular users,returns only published articles
        Task<List<ArticleResponse>> GetAllAsync();
        Task<ArticleResponse> GetByIdAsync(Guid id, bool isAdmin = false);//isAdmin- admins can preview unpublished articles, regular users can't.
        Task<ArticleResponse> CreateAsync(CreateArticleRequest request, Guid authorId);
        Task<ArticleResponse> UpdateAsync(Guid id, UpdateArticleRequest request);
        Task<ArticleResponse> UpdateImageAsync(Guid id, string imageUrl);
        Task DeleteAsync(Guid id);

    }
}
