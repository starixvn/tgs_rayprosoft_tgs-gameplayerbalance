using Tgs.GameFundTransfer.Protos;
using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Core;

public interface IPlayerFormRepository
{
	ValueTask<long> AddAsync(string gameProviderCode, string brandCode, string playerCode, string username, Guid walletId, string currencyCode, CancellationToken cancellationToken = default);

	ValueTask<long?> BrandGetFirstIdAsync(long expansionFormId, PlayerFormState state, CancellationToken cancellationToken = default);

	IAsyncEnumerable<long> BrandPlayerFormAddAsync(IEnumerable<PlayerFormAdd> source, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PlayerForm> BrandQueryAsync(long expansionFormId, PlayerFormState state, long id, int pageSize = 1000, CancellationToken cancellationToken = default);

	ValueTask<PlayerForm?> GetAsync(long id, CancellationToken cancellationToken = default);

	ValueTask<long?> ImportGetFirstIdAsync(long expansionFormId, PlayerFormState state, CancellationToken cancellationToken = default);

	IAsyncEnumerable<long> ImportPlayerFormAddAsync(IEnumerable<PlayerFormAdd> source, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PlayerForm> ImportQueryAsync(long expansionFormId, PlayerFormState state, long id, int pageSize = 1000, CancellationToken cancellationToken = default);

	ValueTask InitializeAsync(CancellationToken cancellationToken = default);

	ValueTask UpdateCompleteAsync(long id, Guid fundTransactionId, string cashTransactionId, Guid cashRespTransactionId,string gameTransactionId, string gameRespTransactionId, string status = "success", CancellationToken cancellationToken = default);

	ValueTask UpdateCompleteAsync(long id, string status = "success", CancellationToken cancellationToken = default);

	ValueTask UpdateFailedAsync(long id, string status = "fail", CancellationToken cancellationToken = default);

	ValueTask UpdateProcessingAsync(long id, CancellationToken cancellationToken = default);
}
