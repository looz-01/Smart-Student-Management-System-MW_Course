using Microsoft.AspNetCore.Http;

namespace StudentMangmentSystem_API.Services.ImageService
{
    public interface IImageService
    {
        Task<string?> SaveImageAsync(IFormFile file, string folder = "uploads");
        Task DeleteImageAsync(string? url);
    }
}