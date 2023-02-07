using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record PlayerFormAddViewModel
{
    [Required]
    public string PlayerCode { get; init; } = default!;
    [Required]
    public string GameProviderCode { get; init; } = default!;
}
