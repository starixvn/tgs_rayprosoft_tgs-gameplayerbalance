using Microsoft.Extensions.Caching.Memory;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories;

internal class BrandCacheRepository(
	IMemoryCache memoryCache,
	IBrandRepository repository) : IBrandRepository
{
	public async ValueTask<Guid> GetIdAsync(
		string code,
		CancellationToken cancellationToken = default)
	{
		var key = $"game:gameplayerbalance:brandcache:get:brandid:{code}";
		if (!memoryCache.TryGetValue(key, out Guid? brandId))
		{
			brandId = await repository
				.GetIdAsync(
					code: code,
					cancellationToken: cancellationToken)
				.ConfigureAwait(false)!;

			if (brandId.HasValue)
			{
				return memoryCache
					.Set(
						key: key,
						value: brandId.Value);
			}
		}

		return brandId ?? throw new Exception($"The brand id does not exist.({code})");
	}
}
