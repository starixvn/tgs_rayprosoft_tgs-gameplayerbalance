namespace Tgs.GamePlayerBalance.Core;

public interface IGameFundTransferTraceRepository
{
	ValueTask<string[]> QueryPlayerCodeAsync(string gameProviderCode, string? brandCode, DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken = default);
}
