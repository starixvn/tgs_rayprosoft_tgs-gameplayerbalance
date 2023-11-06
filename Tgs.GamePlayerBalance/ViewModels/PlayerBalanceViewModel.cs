using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record PlayerBalanceViewModel
{
    [Required]
    public string PlayerCode { get; init; } = default!;
    [Required]
    public string GameProviderCode { get; init; } = default!;
}
