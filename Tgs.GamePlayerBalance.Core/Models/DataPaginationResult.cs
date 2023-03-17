namespace Tgs.GamePlayerBalance.Core.Models;

public record DataPaginationResult<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    long TotalPage = 1,
    long TotalCount = 0);
