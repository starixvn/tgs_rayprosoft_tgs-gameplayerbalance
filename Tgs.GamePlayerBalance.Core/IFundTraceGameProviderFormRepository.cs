using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Core;

public interface IFundTraceGameProviderFormRepository
{
	ValueTask<long> AddAsync(string gameProviderCode, IEnumerable<string> players, CancellationToken cancellationToken = default);

	ValueTask<GameProviderTimeForm?> GetAsync(long id, CancellationToken cancellationToken = default);

	ValueTask InitializeAsync(CancellationToken cancellationToken = default);

	ValueTask UpdateCompleteAsync(long id, string status = "success", CancellationToken cancellationToken = default);

	ValueTask UpdateFailedAsync(long id, string status = "fail", CancellationToken cancellationToken = default);

	ValueTask UpdateProcessingAsync(long id, CancellationToken cancellationToken = default);
}
