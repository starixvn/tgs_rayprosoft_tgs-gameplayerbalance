namespace Tgs.GamePlayerBalance.Core.Models;
public record PlayerInfo(
	string BrandCode,
	string PlayerCode,
	string Username,
	Guid WalletId,
	string CurrencyCode);
