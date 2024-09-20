using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record class FundTraceBrandFormAddViewModel
{
	[Required]
	public string GameProviderCode { get; init; } = default!;

	[Required]
	public string BrandCode { get; init; } = default!;

	public DateTimeOffset? StartTime { get; init; }

	public DateTimeOffset? EndTime { get; init; }
}
