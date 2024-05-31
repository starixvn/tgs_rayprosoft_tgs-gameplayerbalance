namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record FundsTransferInQuery(
	string GameProviderCode,
	string BrandCode,
	string PlayerCode,
	Guid WalletId,
	string CurrencyCode);
