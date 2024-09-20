using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundTraceGameProviderFormProcessingCommandService(
	ILogger<FundTraceGameProviderFormProcessingCommandService> logger,
	IFundTraceGameProviderFormRepository fundTraceGameProviderFormRepository,
	IPlayerFormRepository playerFormRepository,
	ICommandService<FundTraceGameProviderFormProcessingPublishCommand> service) : ICommandService<FundTraceGameProviderFormProcessingCommand>
{
	public async ValueTask ExecuteAsync(
		FundTraceGameProviderFormProcessingCommand command,
		CancellationToken cancellationToken = default)
	{
		var form = await fundTraceGameProviderFormRepository.GetAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (form == null)
		{
			logger.LogError("This game provider time form does not exist.({id})", command.Id);
			throw new Exception($"This game provider time form does not exist.({command.Id})");
		}

		if (form.State != FundTraceGameProviderFormState.Processing)
		{
			logger.LogError("The game provider time form status is not processing.({id})", command.Id);
			throw new Exception($"The game provider time form status is not processing.({command.Id})");
		}

		try
		{
			var filterId = await playerFormRepository.FundTraceGameProviderGetFirstIdAsync(
				expansionFormId: form.Id,
				state: PlayerFormState.Queued,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? 0;

			if (filterId > 0)
			{
				filterId--;
				await service.ExecuteAsync(
					command: new FundTraceGameProviderFormProcessingPublishCommand(
						ExpansionFormId: form.Id,
						FilterId: filterId),
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			await fundTraceGameProviderFormRepository.UpdateCompleteAsync(
				id: command.Id,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unknown error occurred.({id})", command.Id);

			await fundTraceGameProviderFormRepository.UpdateFailedAsync(
				id: command.Id,
				status: ex.Message,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
