using Microsoft.Extensions.Caching.Memory;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories;

internal class BrandCacheRepository : IBrandRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly IBrandRepository _repository;

    public BrandCacheRepository(
        IMemoryCache memoryCache,
        IBrandRepository repository
        )
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask<Guid> GetIdAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var key = $"game:gameplayerbalance:brandcache:get:brandid:{code}";
        if (!_memoryCache.TryGetValue(key, out Guid? brandId))
        {
            brandId = await _repository
                .GetIdAsync(
                    code: code,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)!;

            if (brandId.HasValue)
            {
                return _memoryCache
                    .Set(
                        key: key,
                        value: brandId.Value);
            }
        }
        return brandId
            ?? throw new Exception($"The brand id does not exist.({code})");
    }
}
