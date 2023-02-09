using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.ViewModels;

public record BrandFormQueryViewModel(
    string BrandCode,
    BrandFormState? State,
    int pageSize = 100);