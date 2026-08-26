using StudentManagementSystem.API;
using StudentManagementSystem.DTOs.Common;

namespace StudentManagementSystem.API.Tests;

public class ResponseApiTests
{
    [Fact]
    public void Ok_SetsSuccessAndStatusCode()
    {
        var result = ResponseApi<string>.Ok("data", "Done.");

        Assert.True(result.IsSuccess);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Done.", result.Message);
        Assert.Equal("data", result.Value);
    }

    [Fact]
    public void BadRequest_SetsFailureAndStatusCode()
    {
        var result = ResponseApi<string>.BadRequest("Invalid.");

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Invalid.", result.Message);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Unauthorized_SetsStatusCode()
    {
        var result = ResponseApi<object>.Unauthorized();

        Assert.False(result.IsSuccess);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public void NotFound_SetsStatusCode()
    {
        var result = ResponseApi<object>.NotFound("Missing.");

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void Conflict_SetsStatusCode()
    {
        var result = ResponseApi<object>.Conflict("Conflict.");

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public void CreatedAt_SetsStatusCode()
    {
        var result = ResponseApi<object>.CreatedAt(null, "Created.");

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.StatusCode);
    }

    [Fact]
    public void NoContent_SetsStatusCode()
    {
        var result = ResponseApi<object>.NoContent(null);

        Assert.True(result.IsSuccess);
        Assert.Equal(204, result.StatusCode);
    }
}