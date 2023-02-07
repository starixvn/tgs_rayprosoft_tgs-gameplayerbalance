namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerFormAdd(
    long FormId,
    long BrandFormId,
    Guid GameProviderId,
    string GameProviderCode,
    Guid BrandId,
    string BrandCode,
    string PlayerCode,
    string Username,
    Guid WalletId,
    string CurrencyCode);
