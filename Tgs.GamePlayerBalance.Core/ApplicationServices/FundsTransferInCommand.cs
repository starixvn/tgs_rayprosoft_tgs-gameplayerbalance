namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record FundsTransferInCommand(
    Guid GameProviderId,
    string GameProviderCode,
    string BrandCode,
    string PlayerCode,
    Guid WalletId,
    string CurrencyCode);