using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundsTransferInGetService(
	IPlayerFundRepository playerFund) : IGetService<FundsTransferInQuery, FundsTransferInResult>
{
	public async ValueTask<FundsTransferInResult> GetAsync(
		FundsTransferInQuery query,
		CancellationToken cancellationToken = default)
	{
		var reply = await playerFund.FundInZeroResultAsync(
			gameProviderCode: query.GameProviderCode,
			playerCode: query.PlayerCode,
			walletId: query.WalletId,
			currencyCode: query.CurrencyCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return new FundsTransferInResult(
			FundTransactionId: reply.FundTransactionId,
			TransactionId: reply.TransactionId,
			ExternalOrderId: reply.ExternalOrderId);
	}
}
