using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.ViewModels;

public record ImportFormViewModel(
	string Id,
	Guid GameProviderId,
	string GameProviderCode,
	ImportFormState State,
	string Status,
	IEnumerable<ImportFormPlayer> Players,
	DateTime CreatedOn,
	DateTime UpdateOn,
	DateTime? StartOn,
	DateTime? EndOn,
	DateTime? ExpireOn);
