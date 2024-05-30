using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories;

internal class GameProviderRepository(
	IGameCommandClient client) : IGameProviderRepository
{
	public async ValueTask<GameProviderInfo> GetAsync(Guid id, CancellationToken cancellationToken = default)
		=> await client.GameProviderGetAsync(
			gameProviderId: id,
			cancellationToken: cancellationToken)
		.ConfigureAwait(false)
		?? throw new NullReferenceException($"Game Provider does not exist.({id})");

	public async ValueTask<Guid> GetIdAsync(
		string code,
		CancellationToken cancellationToken = default)
		=> await client.GameProviderGetGameProviderIdAsync(
			gameProviderCode: code,
			cancellationToken: cancellationToken)
		.ConfigureAwait(false)
		?? throw new NullReferenceException($"Game Provider does not exist.({code})");
}
