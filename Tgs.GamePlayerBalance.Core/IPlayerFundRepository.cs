using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Core;

public interface IPlayerFundRepository
{
	ValueTask<PlayerFundResult> FundInZeroResultAsync(string gameProviderCode, string playerCode, Guid walletId, string currencyCode, CancellationToken cancellationToken = default);
}
