using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Core;

public interface IFundTraceBrandFormRepository
{
	ValueTask<long> AddAsync(string gameProviderCode, string brandCode, IEnumerable<string> players, CancellationToken cancellationToken = default);

	ValueTask<FundTraceBrandForm?> GetAsync(long id, CancellationToken cancellationToken = default);

	ValueTask InitializeAsync(CancellationToken cancellationToken = default);

	ValueTask UpdateCompleteAsync(long id, string status = "success", CancellationToken cancellationToken = default);

	ValueTask UpdateFailedAsync(long id, string status = "fail", CancellationToken cancellationToken = default);

	ValueTask UpdateProcessingAsync(long id, CancellationToken cancellationToken = default);
}
