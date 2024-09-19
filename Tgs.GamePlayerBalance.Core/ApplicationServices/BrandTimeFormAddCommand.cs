namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record class BrandTimeFormAddCommand(
	string GameProviderCode,
	string BrandCode,
	DateTimeOffset StartTime,
	DateTimeOffset EndTime);
