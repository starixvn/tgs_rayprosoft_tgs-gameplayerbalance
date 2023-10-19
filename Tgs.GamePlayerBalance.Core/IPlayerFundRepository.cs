using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Core;

public interface IPlayerFundRepository
{
    ValueTask<PlayerFundResult> FundInZeroResultAsync(Guid gameProviderId, string gameProviderCode, string brandCode, string playerCode, Guid walletId, string currencyCode, CancellationToken cancellationToken = default);
}
