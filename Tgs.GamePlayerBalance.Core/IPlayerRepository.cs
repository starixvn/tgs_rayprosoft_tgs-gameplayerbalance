namespace Tgs.GamePlayerBalance.Core;
public interface IPlayerRepository
{
	IAsyncEnumerable<Models.PlayerInfo> QueryAsync(Guid brandId, int pageIndex = 1, int pageSize = 500, CancellationToken cancellationToken = default);
	ValueTask<Models.PlayerInfo?> GetAsync(string playerCode, CancellationToken cancellationToken = default);
}
