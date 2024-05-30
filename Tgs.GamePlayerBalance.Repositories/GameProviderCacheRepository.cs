using Microsoft.Extensions.Caching.Memory;
using Tgs.GameCommand.Common;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories;

internal class GameProviderCacheRepository(
	IMemoryCache memoryCache,
	IGameProviderRepository repository) : IGameProviderRepository
{
	public async ValueTask<GameProviderInfo> GetAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var key = $"game:gameplayerbalance:gameprovidercache:get:{id}:info";
		if (!memoryCache.TryGetValue(key, out GameProviderInfo? info))
		{
			info = await repository.GetAsync(
				id: id,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? throw new NullReferenceException($"Game Provider does not exist.({id})");

			return memoryCache.Set(
				key: key,
				value: info);
		}

		return info ?? throw new NullReferenceException($"Game Provider does not exist.({id})");
	}

	public async ValueTask<Guid> GetIdAsync(
		string code,
		CancellationToken cancellationToken = default)
	{
		var key = $"game:gameplayerbalance:gameprovidercache:get:gameproviderid:{code}";
		if (!memoryCache.TryGetValue(key, out Guid? gameProviderId))
		{
			gameProviderId = await repository.GetIdAsync(
				code: code,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return memoryCache.Set(
				key: key,
				value: gameProviderId.Value);
		}

		return gameProviderId ?? throw new Exception($"Game Provider does not exist.({code})");
	}
}
