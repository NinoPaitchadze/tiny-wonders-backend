namespace TinyWondersAPI.DTOs.Categories
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
