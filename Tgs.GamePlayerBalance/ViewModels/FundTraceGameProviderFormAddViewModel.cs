using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record class FundTraceGameProviderFormAddViewModel
{
	[Required]
	public string GameProviderCode { get; init; } = default!;

	public DateTimeOffset? StartTime { get; init; }

	public DateTimeOffset? EndTime { get; init; }
}
