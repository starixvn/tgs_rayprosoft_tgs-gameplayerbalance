using Tgs.GameFundTransfer.Client;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Models;

namespace Tgs.GamePlayerBalance.Repositories;

internal class PlayerFundRepository(
	IGameFundTransferClient client) : IPlayerFundRepository
{
	public async ValueTask<PlayerFundResult> FundInZeroResultAsync(string gameProviderCode, string playerCode, Guid walletId, string currencyCode, CancellationToken cancellationToken = default)
	{
		var reply = await client.FundInZeroResultAsync(
			gameProviderCode: gameProviderCode,
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
