namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record class FundTraceFormAddCommand(
	string GameProviderCode,
	string? BrandCode,
	DateTimeOffset? StartTime,
	DateTimeOffset? EndTime);
