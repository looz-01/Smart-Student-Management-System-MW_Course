using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace StudentMangmentSystem_API.Services.ImageService
{
    public class ImageService : IImageService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        private const long MaxFileSize = 2 * 1024 * 1024;

        private readonly IWebHostEnvironment _environment;

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SaveImageAsync(IFormFile file, string folder = "uploads")
        {
            if (file == null || file.Length == 0) return null;

            if (file.Length > MaxFileSize)
                throw new InvalidOperationException("Image size must be less than 2 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("Unsupported image type.");

            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadFolder = Path.Combine(rootPath, folder);
            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadFolder, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folder}/{fileName}";
        }

        public Task DeleteImageAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return Task.CompletedTask;

            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var filePath = Path.Combine(rootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }
    }
}