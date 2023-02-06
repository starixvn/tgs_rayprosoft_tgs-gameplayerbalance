namespace Tgs.GamePlayerBalance.Core;

public interface IGameProviderRepository
{
    ValueTask<Guid> GetIdAsync(
        string code,
        CancellationToken cancellationToken = default);
}
