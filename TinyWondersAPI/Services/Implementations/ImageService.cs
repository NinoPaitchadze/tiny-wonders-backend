using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using TinyWondersAPI.Services.Interfaces;

namespace TinyWondersAPI.Services.Implementations;

public class ImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public ImageService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            throw new ArgumentException("Only JPEG, PNG and WebP images are allowed");

        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File size must be under 5MB");

        using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "tinywonders/articles",
            Transformation = new Transformation()
                .Width(1200)
                .Height(630)
                .Crop("fill")
                .Quality("auto")
                .FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Upload failed: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        var uri = new Uri(imageUrl);
        var segments = uri.Segments;
        var publicId = string.Join("", segments
            .SkipWhile(s => s != "tinywonders/")
            .Take(2))
            .TrimEnd('/');

        if (string.IsNullOrEmpty(publicId)) return;

        var deleteParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deleteParams);
    }
}
