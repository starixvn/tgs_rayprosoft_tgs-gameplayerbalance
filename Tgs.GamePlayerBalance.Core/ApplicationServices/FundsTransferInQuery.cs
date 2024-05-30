namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record FundsTransferInQuery(
	Guid GameProviderId,
	string GameProviderCode,
	string BrandCode,
	string PlayerCode,
	Guid WalletId,
	string CurrencyCode);
