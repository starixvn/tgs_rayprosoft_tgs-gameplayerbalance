namespace Tgs.GamePlayerBalance.Core.Models;

public record BrandForm(
    long Id,
    Guid GameProviderId,
    string GameProviderCode,
    Guid BrandId,
    string BrandCode,
    BrandFormState State,
    string Status,
    DateTime CreatedOn,
    DateTime UpdateOn,
    DateTime? StartOn,
    DateTime? EndOn,
    DateTime? ExpireOn);
