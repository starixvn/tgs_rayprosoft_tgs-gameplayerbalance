using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record AwcAelottoViewModel
{
    [Required]
    public string BrandCode { get; init; } = default!;
}
