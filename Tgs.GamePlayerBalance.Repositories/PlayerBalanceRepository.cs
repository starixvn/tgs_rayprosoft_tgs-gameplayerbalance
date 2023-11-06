using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GameProviders.Client;

namespace Tgs.GamePlayerBalance.Repositories;

internal class PlayerBalanceRepository : IPlayerBalanceRepository
{
    private readonly IGameProvidersClient _client;

    public PlayerBalanceRepository(
        IGameProvidersClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<PlayerBalance> GetAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default)
    {
        var response = await _client.PlayerBalanceAsync(
            gameProviderCode: gameProviderCode,
            playerCode: playerCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new PlayerBalance(
            response.Balance,
            response.FreeBalance,
            response.OutstandingBalance);
    }
}
