using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.ViewModels;

public record BrandFormViewModel(
    string Id,
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
