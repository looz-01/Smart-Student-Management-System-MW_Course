using StudentMangmentSystemDTO_s.Common;

namespace StudentMangmentSystem_API.Extensions
{
    public static class PagedResultFactory
    {
        public static PagedResult<T> Create<T>(IEnumerable<T> items, int totalCount, PageRequest request)
        {
            request.Normalize();
            return new PagedResult<T>
            {
                Items = items.ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }
    }
}