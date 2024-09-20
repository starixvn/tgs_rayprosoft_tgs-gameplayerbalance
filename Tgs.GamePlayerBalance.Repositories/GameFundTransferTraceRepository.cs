using Tgs.GameFund.Client;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories;

internal class GameFundTransferTraceRepository(IGameFundClient gameFundClient) : IGameFundTransferTraceRepository
{
	public ValueTask<string[]> QueryPlayerCodeAsync(string gameProviderCode, string? brandCode, DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken = default)
		=> gameFundClient.GameFundTransferTraceQueryAsync(gameProviderCode, brandCode, startTime, endTime, cancellationToken);
}
