namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerFormAdd(
	long FormId,
	long ExpansionFormId,
	string GameProviderCode,
	string BrandCode,
	string PlayerCode,
	string Username,
	Guid WalletId,
	string CurrencyCode);
