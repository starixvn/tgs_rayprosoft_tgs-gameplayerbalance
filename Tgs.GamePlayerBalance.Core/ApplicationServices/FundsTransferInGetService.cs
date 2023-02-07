using Tgs.GameFundTransfer.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundsTransferInGetService : IGetService<FundsTransferInQuery, FundsTransferInResult>
{
    private readonly IGameFundTransferClient _client;

    public FundsTransferInGetService(
        IGameFundTransferClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<FundsTransferInResult> GetAsync(
        FundsTransferInQuery query,
        CancellationToken cancellationToken = default)
    {
        var reply = await _client
                    .FundInZeroResultAsync(
                        gameProviderId: query.GameProviderId,
                        gameProviderCode: query.GameProviderCode,
                        brandCode: query.BrandCode,
                        playerCode: query.PlayerCode,
                        walletId: query.WalletId,
                        currencyCode: query.CurrencyCode,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

        return new FundsTransferInResult(
            StateCode: reply.State,
            Message: reply.Message,
            FundTransactionId: reply.FundTransactionId.ToGuid(),
            TransactionId: reply.TransactionId.ToGuid(),
            ExternalTransactionId: reply.ExternalTransactionId);
    }
}
