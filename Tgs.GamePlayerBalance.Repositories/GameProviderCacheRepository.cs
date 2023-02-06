using Microsoft.Extensions.Caching.Memory;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories;

internal class GameProviderCacheRepository : IGameProviderRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly IGameProviderRepository _repository;

    public GameProviderCacheRepository(
        IMemoryCache memoryCache,
        IGameProviderRepository repository)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask<Guid> GetIdAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var key = $"game:gameplayerbalance:gameprovidercache:get:gameproviderid:{code}";
        if (!_memoryCache.TryGetValue(key, out Guid? gameProviderId))
        {
            gameProviderId = await _repository
                .GetIdAsync(
                    code: code,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (gameProviderId.HasValue)
            {
                return _memoryCache
                    .Set(
                        key: key,
                        value: gameProviderId.Value);
            }
        }
        return gameProviderId
            ?? throw new Exception($"Game Provider does not exist.({code})");
    }
}
