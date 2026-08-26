using Microsoft.AspNetCore.Http;

namespace StudentManagementSystem.API.Services.ImageService
{
    public interface IImageService
    {
        Task<string?> SaveImageAsync(IFormFile file, string folder = "uploads");
        Task DeleteImageAsync(string? url);
    }
}