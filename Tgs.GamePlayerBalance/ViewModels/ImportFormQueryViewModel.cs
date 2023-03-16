using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.ViewModels;

public record ImportFormQueryViewModel(
    string GameProviderCode,
    ImportFormState? State,
    int PageSize = 100);