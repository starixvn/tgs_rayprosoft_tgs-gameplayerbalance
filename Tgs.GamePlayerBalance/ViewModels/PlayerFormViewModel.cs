using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.ViewModels;

public record PlayerFormViewModel(
	string Id,
	string ExpansionFormId,
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
