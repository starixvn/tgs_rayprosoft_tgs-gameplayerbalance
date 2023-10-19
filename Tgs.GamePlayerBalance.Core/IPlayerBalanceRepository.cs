using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Core;

public interface IPlayerBalanceRepository
{
    ValueTask<PlayerBalance> GetAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default);
}
