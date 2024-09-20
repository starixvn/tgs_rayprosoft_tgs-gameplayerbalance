namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record class FundTraceBrandFormAddCommand(
	string GameProviderCode,
	string BrandCode,
	DateTimeOffset? StartTime,
	DateTimeOffset? EndTime);
