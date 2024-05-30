namespace Tgs.GamePlayerBalance.ViewModels;

public record DataPaginationViewModel<T>(
	IEnumerable<T> Items,
	int Page,
	int PageSize,
	long TotalPage = 1,
	long TotalCount = 0);
