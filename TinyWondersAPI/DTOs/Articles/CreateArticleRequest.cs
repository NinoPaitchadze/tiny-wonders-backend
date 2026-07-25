using System.Diagnostics.Eventing.Reader;

namespace TinyWondersAPI.DTOs.Articles
{
    public class CreateArticleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public bool IsPublished { get; set; } = false;
    }
}
