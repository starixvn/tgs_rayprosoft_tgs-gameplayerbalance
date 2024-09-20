namespace Tgs.GamePlayerBalance.Core.Models;

public record class FundTraceGameProviderForm(
	long Id,
	string GameProviderCode,
	FundTraceGameProviderFormState State,
	string Status,
	IEnumerable<FundTraceGameProviderFormPlayer> Players,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
