using Microsoft.AspNetCore.Http;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Services.ImageService;
using StudentManagementSystem.API.Tests.Infrastructure;

namespace StudentManagementSystem.API.Tests;

public class ImageServiceTests : IDisposable
{
    private readonly TestServiceProvider _provider = new();

    private static IFormFile CreateFormFile(string fileName, string contentType, byte[] content)
        => new FormFile(new MemoryStream(content), 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

    [Fact]
    public async Task Save_ValidPng_ReturnsUrl()
    {
        var imageService = _provider.GetService<IImageService>();
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4 };

        var url = await imageService.SaveImageAsync(CreateFormFile("photo.png", "image/png", png), "uploads/students");

        Assert.NotNull(url);
        Assert.StartsWith("/uploads/students/", url);
    }

    [Fact]
    public async Task Save_FakePngWithWrongContent_Throws()
    {
        var imageService = _provider.GetService<IImageService>();
        var notAPng = new byte[] { 0x89, 0x50, 0x00, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4 };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            imageService.SaveImageAsync(CreateFormFile("fake.png", "image/png", notAPng)));

        Assert.Contains("not a valid image", ex.Message);
    }

    [Fact]
    public async Task Save_HtmlContentDisguisedAsJpg_Throws()
    {
        var imageService = _provider.GetService<IImageService>();
        var html = "<html><script>alert(1)</script></html>"u8.ToArray();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            imageService.SaveImageAsync(CreateFormFile("evil.jpg", "image/jpeg", html)));

        Assert.Contains("not a valid image", ex.Message);
    }

    [Fact]
    public async Task Save_UnsupportedExtension_Throws()
    {
        var imageService = _provider.GetService<IImageService>();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            imageService.SaveImageAsync(CreateFormFile("evil.exe", "application/octet-stream", new byte[] { 1, 2, 3 })));

        Assert.Contains("Unsupported image type", ex.Message);
    }

    [Fact]
    public async Task Save_ContentTypeMismatch_Throws()
    {
        var imageService = _provider.GetService<IImageService>();
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4 };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            imageService.SaveImageAsync(CreateFormFile("photo.png", "text/html", png)));

        Assert.Contains("does not match", ex.Message);
    }

    public void Dispose() => _provider.Dispose();
}