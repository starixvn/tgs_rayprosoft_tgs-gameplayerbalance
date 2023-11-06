using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundsTransferInGetService : IGetService<FundsTransferInQuery, FundsTransferInResult>
{
    private readonly IPlayerFundRepository _playerFund;

    public FundsTransferInGetService(
        IPlayerFundRepository playerFund)
    {
        _playerFund = playerFund ?? throw new ArgumentNullException(nameof(playerFund));
    }

    public async ValueTask<FundsTransferInResult> GetAsync(
        FundsTransferInQuery query,
        CancellationToken cancellationToken = default)
    {
        var reply = await _playerFund.FundInZeroResultAsync(
            gameProviderId: query.GameProviderId,
            gameProviderCode: query.GameProviderCode,
            brandCode: query.BrandCode,
            playerCode: query.PlayerCode,
            walletId: query.WalletId,
            currencyCode: query.CurrencyCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new FundsTransferInResult(
            StateCode: reply.StateCode,
            Message: reply.Message,
            FundTransactionId: reply.FundTransactionId,
            TransactionId: reply.TransactionId,
            ExternalTransactionId: reply.ExternalTransactionId);
    }
}
