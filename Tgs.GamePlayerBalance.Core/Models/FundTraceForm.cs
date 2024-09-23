namespace Tgs.GamePlayerBalance.Core.Models;

public record class FundTraceForm(
	long Id,
	string GameProviderCode,
	string? BrandCode,
	DateTimeOffset? StartTime,
	DateTimeOffset? EndTime,
	FundTraceFormState State,
	string Status,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
