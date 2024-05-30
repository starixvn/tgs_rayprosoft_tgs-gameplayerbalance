using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Core;

public interface IBrandFormRepository
{
	ValueTask<long> AddAsync(
		Guid gameProviderId,
		string gameProviderCode,
		Guid brandId,
		string brandCode,
		CancellationToken cancellationToken = default);

	ValueTask<BrandForm?> GetAsync(
		long id,
		CancellationToken cancellationToken = default);

	ValueTask InitializeAsync(
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<BrandForm> QueryAsync(
	  Guid brandId,
	  BrandFormState? state,
	  int pageSize = 100,
	  CancellationToken cancellationToken = default);

	ValueTask UpdateCompleteAsync(
					long id,
		string status = "success",
		CancellationToken cancellationToken = default);

	ValueTask UpdateFailedAsync(
		long id,
		string status = "fail",
		CancellationToken cancellationToken = default);

	ValueTask UpdateProcessingAsync(
		long id,
		CancellationToken cancellationToken = default);
}
