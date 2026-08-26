using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using StudentManagementSystem.API.Common;

namespace StudentManagementSystem.API.Services.ImageService
{
    public class ImageService : IImageService
    {
        private static readonly Dictionary<string, string[]> AllowedImageTypes = new()
        {
            [".jpg"] = new[] { "image/jpeg" },
            [".jpeg"] = new[] { "image/jpeg" },
            [".png"] = new[] { "image/png" },
            [".gif"] = new[] { "image/gif" },
            [".webp"] = new[] { "image/webp" },
            [".bmp"] = new[] { "image/bmp" }
        };

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
                throw new AppException("Image size must be less than 2 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageTypes.TryGetValue(extension, out var allowedMimeTypes))
                throw new AppException("Unsupported image type.");

            if (string.IsNullOrWhiteSpace(file.ContentType) ||
                !allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                throw new AppException("The file content does not match a supported image type.");

            var uploadRoot = GetUploadRoot();
            var uploadFolder = Path.Combine(uploadRoot, folder);
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

            var uploadRoot = GetUploadRoot();
            var fullPath = Path.GetFullPath(Path.Combine(uploadRoot, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            // Prevent path traversal outside the uploads root.
            if (!fullPath.StartsWith(uploadRoot, StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }

        private string GetUploadRoot()
        {
            return Path.GetFullPath(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"));
        }
    }
}