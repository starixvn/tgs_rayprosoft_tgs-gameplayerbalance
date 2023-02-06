using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record BrandViewModel
{
    [Required]
    public string BrandCode { get; init; } = default!;
    [Required]
    public string GameProviderCode { get; init; } = default!;
}
