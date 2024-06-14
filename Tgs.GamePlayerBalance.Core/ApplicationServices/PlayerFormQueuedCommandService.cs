using Microsoft.Extensions.Logging;
using Tgs.GameFundTransfer.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFormQueuedCommandService(
	ILogger<PlayerFormQueuedCommandService> logger,
	IGameFundTransferClient gameFundTransferClient,
	IPlayerFormRepository repository) : ICommandService<PlayerFormQueuedCommand>
{
	public async ValueTask ExecuteAsync(PlayerFormQueuedCommand command, CancellationToken cancellationToken = default)
	{
		var form = await repository.GetAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (form == null)
		{
			logger.LogError("This player form does not exist.({id})", command.Id);
			throw new Exception($"This player form does not exist.({command.Id})");
		}

		if (form.State != PlayerFormState.Queued)
		{
			logger.LogError("The player form status is not queued.({id})", command.Id);
			throw new Exception($"The player form status is not queued.({command.Id})");
		}

		await repository.UpdateProcessingAsync(
			id: form.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		try
		{
			var playerBalance = await gameFundTransferClient.FundBalanceAsync(
				gameProviderCode: form.GameProviderCode,
				playerCode: form.PlayerCode,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (playerBalance.FreeBalance > 0)
			{
				var result = await gameFundTransferClient.FundTransferInOneClickAsync(
					gameProviderCode: form.GameProviderCode,
					playerCode: form.PlayerCode,
					walletId: form.WalletId,
					currencyCode: form.CurrencyCode,
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				await repository.UpdateCompleteAsync(
					id: command.Id,
					fundTransactionId: result.FundTransactionId,
					cashTransactionId: result.CashTransactionId,
					cashRespTransactionId: result.CashRespTransactionId,
					gameTransactionId: result.GameTransactionId,
					gameRespTransactionId: result.GameRespTransactionId,
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
			else
			{
				await repository.UpdateCompleteAsync(
					id: form.Id,
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unknown error occurred.({id})", form.Id);

			await repository.UpdateFailedAsync(
				id: command.Id,
				status: ex.Message,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
