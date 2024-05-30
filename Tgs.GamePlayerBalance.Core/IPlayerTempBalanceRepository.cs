namespace Tgs.GamePlayerBalance.Core;

public interface IPlayerTempBalanceRepository
{
	ValueTask AddAsync(string gameProviderCode, string playerCode, decimal balance, decimal freeBalance, decimal outstandingBalance, CancellationToken cancellationToken = default);

	ValueTask DeleteAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default);

	ValueTask<bool> ExistAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default);

	ValueTask InitializeAsync(CancellationToken cancellationToken = default);
}
