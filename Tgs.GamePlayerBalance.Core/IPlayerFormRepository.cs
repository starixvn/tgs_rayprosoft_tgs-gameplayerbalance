using Tgs.GameFundTransfer.Protos;
using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Core;

public interface IPlayerFormRepository
{
	ValueTask<long> AddAsync(
		Guid gameProviderId,
		string gameProviderCode,
		Guid brandId,
		string brandCode,
		string playerCode,
		string username,
		Guid walletId,
		string currencyCode,
		CancellationToken cancellationToken = default);

	ValueTask<long?> BrandGetFirstIdAsync(
		long expansionFormId,
		PlayerFormState state,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<long> BrandPlayerFormAddAsync(
		IEnumerable<PlayerFormAdd> source,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<PlayerForm> BrandQueryAsync(
		long expansionFormId,
		PlayerFormState? state,
		string? playerCode,
		int page = 1,
		int pageSize = 100,
		CancellationToken cancellationToken = default);

	ValueTask<long> BrandQueryCountAsync(
		long expansionFormId,
		PlayerFormState? state,
		string? playerCode,
	  CancellationToken cancellationToken = default);

	IAsyncEnumerable<long> BrandQueryIdAsync(
		long expansionFormId,
		PlayerFormState state,
		long id,
		int pageSize = 1000,
		CancellationToken cancellationToken = default);

	ValueTask<PlayerForm?> GetAsync(
		long id,
		CancellationToken cancellationToken = default);

	ValueTask<long?> ImportGetFirstIdAsync(
		long expansionFormId,
		PlayerFormState state,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<long> ImportPlayerFormAddAsync(
		IEnumerable<PlayerFormAdd> source,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<PlayerForm> ImportQueryAsync(
		long expansionFormId,
		PlayerFormState? state,
		string? playerCode,
		int page = 1,
		int pageSize = 100,
		CancellationToken cancellationToken = default);

	ValueTask<long> ImportQueryCountAsync(
		long expansionFormId,
		PlayerFormState? state,
		string? playerCode,
	  CancellationToken cancellationToken = default);

	IAsyncEnumerable<long> ImportQueryIdAsync(
			long expansionFormId,
		PlayerFormState state,
		long id,
		int pageSize = 1000,
		CancellationToken cancellationToken = default);

	ValueTask InitializeAsync(
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<PlayerForm> QueryAsync(
		string playerCode,
		int pageSize = 100,
		CancellationToken cancellationToken = default);

	ValueTask UpdateCompleteAsync(
		long id,
		StateCode resultStateCode,
		string resultMessage,
		Guid resultFundTransactionId,
		Guid resultTransactionId,
		string resultExternalTransactionId,
		string status = "success",
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
