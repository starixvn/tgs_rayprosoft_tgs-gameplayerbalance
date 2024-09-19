namespace Tgs.GamePlayerBalance.Core.Models;

public record class GameProviderTimeForm(
	long Id,
	string GameProviderCode,
	DateTime StartTime,
	DateTime EndTime,
	GameProviderTimeFormState State,
	string Status,
	IEnumerable<GameProviderTimeFormPlayer> Players,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
