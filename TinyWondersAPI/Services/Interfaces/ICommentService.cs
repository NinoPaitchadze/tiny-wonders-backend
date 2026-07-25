using TinyWondersAPI.DTOs.Comments;
namespace TinyWondersAPI.Services.Interfaces
{
    public interface ICommentService
    {
        Task<List<CommentResponse>> GetAllForArticleAsync(Guid articleId);
        Task<CommentResponse> CreateAsync(Guid articleId, CreateCommentRequest request, Guid userId);
        Task DeleteAsync(Guid commentId, Guid userId, bool isAdmin);
    }
}
