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

    IAsyncEnumerable<long> BrandAddAsync(
        IEnumerable<PlayerFormAdd> source,
        CancellationToken cancellationToken = default);

    ValueTask<PlayerForm?> GetAsync(
        long id,
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
