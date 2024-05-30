namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record ImportFormAddCommand(
	string GameProviderCode,
	IEnumerable<string> Players);
