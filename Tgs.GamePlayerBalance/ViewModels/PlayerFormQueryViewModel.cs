namespace Tgs.GamePlayerBalance.ViewModels;

public record PlayerFormQueryViewModel(
	string PlayerCode,
	int PageSize = 100);
