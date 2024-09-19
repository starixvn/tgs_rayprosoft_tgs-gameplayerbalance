using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record class FundTraceGameProviderFormAddViewModel
{
	[Required]
	public string GameProviderCode { get; init; } = default!;
}
