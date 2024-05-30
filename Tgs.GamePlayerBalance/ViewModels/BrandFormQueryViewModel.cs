using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.ViewModels;

public record BrandFormQueryViewModel(
	string BrandCode,
	BrandFormState? State,
	int PageSize = 100);
