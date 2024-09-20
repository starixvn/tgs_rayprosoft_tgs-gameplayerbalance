namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record class FundTraceGameProviderFormAddCommand(
	string GameProviderCode,
	DateTimeOffset? StartTime,
	DateTimeOffset? EndTime);
