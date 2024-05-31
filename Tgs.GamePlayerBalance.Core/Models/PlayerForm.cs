namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerForm(
	long Id,
	long ExpansionFormId,
	string GameProviderCode,
	string BrandCode,
	string PlayerCode,
	string Username,
	Guid WalletId,
	string CurrencyCode,
	PlayerFormState State,
	string Status,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn,
	PlayerFormFundResult? Result);
