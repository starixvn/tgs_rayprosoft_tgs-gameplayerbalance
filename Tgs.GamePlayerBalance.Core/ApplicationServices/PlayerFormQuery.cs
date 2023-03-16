namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerFormQuery(
    string PlayerCode,
    int PageSize = 100);
