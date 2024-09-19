using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record class BrandTimeFormAddViewModel
{
	[Required]
	public string BrandCode { get; init; } = default!;
	[Required]
	public string GameProviderCode { get; init; } = default!;
	[Required]
	public DateTimeOffset StartTime { get; init; }
	[Required]
	public DateTimeOffset EndTime { get; init; }
}
