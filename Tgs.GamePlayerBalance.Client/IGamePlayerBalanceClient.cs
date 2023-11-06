using Tgs.GamePlayerBalance.Common.Protos;

namespace Tgs.GamePlayerBalance.Client;

public interface IGamePlayerBalanceClient
{
    public ValueTask DeletePlayerBalanceAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default);

    public ValueTask<PlayerBalanceReply> GetPlayerBalanceAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default);
}
