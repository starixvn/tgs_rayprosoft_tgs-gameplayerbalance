namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerForm(
	long Id,
	long ExpansionFormId,
	Guid GameProviderId,
	string GameProviderCode,
	Guid BrandId,
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
