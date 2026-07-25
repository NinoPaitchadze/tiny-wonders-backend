namespace TinyWondersAPI.Services.Interfaces;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile file);
    Task DeleteImageAsync(string imageUrl);
}
