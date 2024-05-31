namespace Tgs.GamePlayerBalance.Core.Models;

public record ImportForm(
	long Id,
	string GameProviderCode,
	ImportFormState State,
	string Status,
	IEnumerable<ImportFormPlayer> Players,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
