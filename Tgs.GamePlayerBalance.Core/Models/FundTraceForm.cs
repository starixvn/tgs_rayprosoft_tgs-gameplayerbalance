namespace Tgs.GamePlayerBalance.Core.Models;

public record class FundTraceForm(
	long Id,
	string GameProviderCode,
	FundTraceFormState State,
	string Status,
	IEnumerable<FundTraceFormPlayer> Players,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
