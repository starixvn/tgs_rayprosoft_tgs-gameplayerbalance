namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerFormAdd(
    long FormId,
    long ExpansionFormId,
    Guid GameProviderId,
    string GameProviderCode,
    Guid BrandId,
    string BrandCode,
    string PlayerCode,
    string Username,
    Guid WalletId,
    string CurrencyCode);
