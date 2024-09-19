namespace Tgs.GamePlayerBalance.Core.Models;

public record class BrandTimeForm(
	long Id,
	string GameProviderCode,
	string BrandCode,
	BrandTimeFormState State,
	string Status,
	IEnumerable<BrandTimeFormPlayer> Players,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
