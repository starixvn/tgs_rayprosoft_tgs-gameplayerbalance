using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record class FundTraceFormAddViewModel
{
	[Required]
	public string GameProviderCode { get; init; } = default!;

	public string? BrandCode { get; init; }

	public DateTimeOffset? StartTime { get; init; }

	public DateTimeOffset? EndTime { get; init; }
}
