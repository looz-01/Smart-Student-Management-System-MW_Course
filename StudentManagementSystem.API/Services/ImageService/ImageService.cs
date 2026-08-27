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

            await using var stream = file.OpenReadStream();
            if (!HasValidImageSignature(stream, extension))
                throw new AppException("The file is not a valid image.");

            var uploadRoot = GetUploadRoot();
            var uploadFolder = Path.Combine(uploadRoot, folder);
            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadFolder, fileName);

            stream.Position = 0;
            await using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            return $"/{folder}/{fileName}";
        }

        private static bool HasValidImageSignature(Stream stream, string extension)
        {
            var signature = new byte[12];
            var read = stream.Read(signature, 0, signature.Length);
            if (read < 3) return false;

            return extension switch
            {
                ".jpg" or ".jpeg" => read >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF,
                ".png" => read >= 8 &&
                          signature[0] == 0x89 && signature[1] == 0x50 && signature[2] == 0x4E &&
                          signature[3] == 0x47 && signature[4] == 0x0D && signature[5] == 0x0A &&
                          signature[6] == 0x1A && signature[7] == 0x0A,
                ".gif" => read >= 4 && signature[0] == 0x47 && signature[1] == 0x49 && signature[2] == 0x46 && signature[3] == 0x38,
                ".webp" => read >= 12 && signature[0] == 0x52 && signature[1] == 0x49 && signature[2] == 0x46 && signature[3] == 0x46,
                ".bmp" => read >= 2 && signature[0] == 0x42 && signature[1] == 0x4D,
                _ => false
            };
        }

        public Task DeleteImageAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return Task.CompletedTask;

            var uploadRoot = GetUploadRoot();
            var fullPath = Path.GetFullPath(Path.Combine(uploadRoot, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            // Prevent path traversal outside the uploads root.
            if (!fullPath.StartsWith(uploadRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
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