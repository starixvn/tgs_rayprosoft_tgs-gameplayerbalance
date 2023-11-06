using Tgs.GameCommand.Common;

namespace Tgs.GamePlayerBalance.Core;

public interface IGameProviderRepository
{
    ValueTask<GameProviderInfo> GetAsync(Guid id, CancellationToken cancellationToken = default);

    ValueTask<Guid> GetIdAsync(string code, CancellationToken cancellationToken = default);
}
