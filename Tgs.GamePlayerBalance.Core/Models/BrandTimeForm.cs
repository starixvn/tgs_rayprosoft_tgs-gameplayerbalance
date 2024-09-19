namespace Tgs.GamePlayerBalance.Core.Models;

public record class BrandTimeForm(
	long Id,
	string GameProviderCode,
	string BrandCode,
	DateTime StartTime,
	DateTime EndTime,
	BrandTimeFormState State,
	string Status,
	IEnumerable<BrandTimeFormPlayer> Players,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
