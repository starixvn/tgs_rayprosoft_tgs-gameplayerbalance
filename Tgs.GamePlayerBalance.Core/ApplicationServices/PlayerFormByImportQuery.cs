namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerFormByImportQuery(
    long ExpansionFormId,
    PlayerFormState? State,
    string? PlayerCode,
    int Page = 1,
    int PageSize = 100);
