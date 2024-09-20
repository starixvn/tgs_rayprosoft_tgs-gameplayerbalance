namespace Tgs.GamePlayerBalance.Core.Models;

public record class FundTraceBrandForm(
	long Id,
	string GameProviderCode,
	string BrandCode,
	FundTraceBrandFormState State,
	string Status,
	IEnumerable<FundTraceBrandFormPlayer> Players,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
