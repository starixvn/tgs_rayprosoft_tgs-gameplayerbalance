namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerFormByBrandQuery(
    long ExpansionFormId,
    PlayerFormState? State,
    string? PlayerCode,
    int Page = 1,
    int PageSize = 100);
