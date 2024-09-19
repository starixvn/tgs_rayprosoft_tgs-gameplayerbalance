using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class GameProviderTimeFormProcessingCommandService(
	ILogger<GameProviderTimeFormProcessingCommandService> logger,
	IFundTraceGameProviderFormRepository gameProviderTimeFormRepository,
	IPlayerFormRepository playerFormRepository,
	ICommandService<GameProviderTimeFormProcessingPublishCommand> service) : ICommandService<GameProviderTimeFormProcessingCommand>
{
	public async ValueTask ExecuteAsync(
		GameProviderTimeFormProcessingCommand command,
		CancellationToken cancellationToken = default)
	{
		var form = await gameProviderTimeFormRepository.GetAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (form == null)
		{
			logger.LogError("This game provider time form does not exist.({id})", command.Id);
			throw new Exception($"This game provider time form does not exist.({command.Id})");
		}

		if (form.State != GameProviderTimeFormState.Processing)
		{
			logger.LogError("The game provider time form status is not processing.({id})", command.Id);
			throw new Exception($"The game provider time form status is not processing.({command.Id})");
		}

		try
		{
			var filterId = await playerFormRepository.GameProviderTimeGetFirstIdAsync(
				expansionFormId: form.Id,
				state: PlayerFormState.Queued,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? 0;

			if (filterId > 0)
			{
				filterId--;
				await service.ExecuteAsync(
					command: new GameProviderTimeFormProcessingPublishCommand(
						ExpansionFormId: form.Id,
						FilterId: filterId),
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			await gameProviderTimeFormRepository.UpdateCompleteAsync(
				id: command.Id,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unknown error occurred.({id})", command.Id);

			await gameProviderTimeFormRepository.UpdateFailedAsync(
				id: command.Id,
				status: ex.Message,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
