using System.ComponentModel.DataAnnotations;

namespace Tgs.GamePlayerBalance.ViewModels;

public record AwcAwsViewModel
{
    [Required]
    public string BrandCode { get; init; } = default!;
}
