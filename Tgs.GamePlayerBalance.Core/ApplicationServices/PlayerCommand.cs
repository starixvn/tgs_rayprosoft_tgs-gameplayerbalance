namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerCommand(
    string GameProviderCode,
    string BrandCode,
    string PlayerCode,
    Guid WalletId,
    string CurrencyCode);
