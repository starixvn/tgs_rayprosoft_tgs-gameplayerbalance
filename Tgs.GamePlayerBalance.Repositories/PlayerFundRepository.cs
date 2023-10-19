using Tgs.GameFundTransfer.Client;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Repositories;

internal class PlayerFundRepository : IPlayerFundRepository
{
    private readonly IGameFundTransferClient _client;

    public PlayerFundRepository(
        IGameFundTransferClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<PlayerFundResult> FundInZeroResultAsync(Guid gameProviderId, string gameProviderCode, string brandCode, string playerCode, Guid walletId, string currencyCode, CancellationToken cancellationToken = default)
    {
        var reply = await _client.FundInZeroResultAsync(
            gameProviderId: gameProviderId,
            gameProviderCode: gameProviderCode,
            brandCode: brandCode,
            playerCode: playerCode,
            walletId: walletId,
            currencyCode: currencyCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new PlayerFundResult(
            StateCode: reply.State,
            Message: reply.Message,
            FundTransactionId: reply.FundTransactionId.ToGuid(),
            TransactionId: reply.TransactionId.ToGuid(),
            ExternalTransactionId: reply.ExternalTransactionId);
    }
}
