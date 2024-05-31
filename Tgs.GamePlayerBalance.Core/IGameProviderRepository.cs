namespace Tgs.GamePlayerBalance.Core;

public interface IGameProviderRepository
{
	ValueTask<Models.GameProviderInfo> GetAsync(string code, CancellationToken cancellationToken = default);
}
