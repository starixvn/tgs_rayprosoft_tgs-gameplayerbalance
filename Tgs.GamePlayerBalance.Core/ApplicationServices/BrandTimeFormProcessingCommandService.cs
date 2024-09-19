using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandTimeFormProcessingCommandService(
	ILogger<BrandTimeFormProcessingCommandService> logger,
	IBrandTimeFormRepository brandTimeFormRepository,
	IPlayerFormRepository playerFormRepository,
	ICommandService<BrandTimeFormProcessingPublishCommand> service) : ICommandService<BrandTimeFormProcessingCommand>
{
	public async ValueTask ExecuteAsync(
		BrandTimeFormProcessingCommand command,
		CancellationToken cancellationToken = default)
	{
		var form = await brandTimeFormRepository.GetAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (form == null)
		{
			logger.LogError("This brand time form does not exist.({id})", command.Id);
			throw new Exception($"This brand time form does not exist.({command.Id})");
		}

		if (form.State != BrandTimeFormState.Processing)
		{
			logger.LogError("The brand time form status is not processing.({id})", command.Id);
			throw new Exception($"The brand time form status is not processing.({command.Id})");
		}

		try
		{
			var filterId = await playerFormRepository.BrandTimeGetFirstIdAsync(
				expansionFormId: form.Id,
				state: PlayerFormState.Queued,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? 0;

			if (filterId > 0)
			{
				filterId--;
				await service.ExecuteAsync(
					command: new BrandTimeFormProcessingPublishCommand(
						ExpansionFormId: form.Id,
						FilterId: filterId),
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			await brandTimeFormRepository.UpdateCompleteAsync(
				id: command.Id,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unknown error occurred.({id})", command.Id);

			await brandTimeFormRepository.UpdateFailedAsync(
				id: command.Id,
				status: ex.Message,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
