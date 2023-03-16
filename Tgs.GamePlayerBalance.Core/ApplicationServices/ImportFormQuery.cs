namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record ImportFormQuery(
    string GameProviderCode,
    ImportFormState? State,
    int PageSize = 100);
