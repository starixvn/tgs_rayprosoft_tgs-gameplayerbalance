namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record class GameProviderTimeFormAddCommand(
	string GameProviderCode,
	DateTimeOffset StartTime,
	DateTimeOffset EndTime);
