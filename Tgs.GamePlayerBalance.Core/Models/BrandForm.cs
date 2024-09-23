namespace Tgs.GamePlayerBalance.Core.Models;

public record BrandForm(
	long Id,
	string GameProviderCode,
	string BrandCode,
	DateTimeOffset StartTime,
	DateTimeOffset EndTime,
	BrandFormState State,
	string Status,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
