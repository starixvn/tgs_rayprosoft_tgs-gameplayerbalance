namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record BrandFormQuery(
    string BrandCode,
    BrandFormState? State,
    int PageSize = 100);
