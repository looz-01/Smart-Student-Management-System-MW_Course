using StudentManagementSystem.API.Extensions;
using StudentManagementSystem.DTOs.Common;

namespace StudentManagementSystem.API.Tests;

public class PageRequestTests
{
    [Theory]
    [InlineData(0, 0, 1, 10)]
    [InlineData(-5, -1, 1, 10)]
    [InlineData(3, 500, 3, 50)]
    [InlineData(2, 25, 2, 25)]
    public void Normalize_ClampsValues(int pageNumber, int pageSize, int expectedPage, int expectedSize)
    {
        var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = "  x  " };

        request.Normalize();

        Assert.Equal(expectedPage, request.PageNumber);
        Assert.Equal(expectedSize, request.PageSize);
        Assert.Equal("x", request.SearchTerm);
    }

    [Fact]
    public void Normalize_TrimsEmptySearchTermToNull()
    {
        var request = new PageRequest { SearchTerm = "   " };

        request.Normalize();

        Assert.Null(request.SearchTerm);
    }
}

public class PagedResultFactoryTests
{
    [Fact]
    public void Create_ComputesTotalPages()
    {
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };
        request.Normalize();

        var result = PagedResultFactory.Create(new[] { 1, 2, 3, 4, 5 }, 25, request);

        Assert.Equal(5, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNext);
        Assert.False(result.HasPrevious);
    }
}