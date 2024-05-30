using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormProcessingCommandService(
	ILogger<BrandFormProcessingCommandService> logger,
	IBrandFormRepository brandFormRepository,
	IPlayerFormRepository playerFormRepository,
	ICommandService<BrandFormProcessingPublishCommand> service) : ICommandService<BrandFormProcessingCommand>
{
	public async ValueTask ExecuteAsync(
		BrandFormProcessingCommand command,
		CancellationToken cancellationToken = default)
	{
		var form = await brandFormRepository.GetAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (form == null)
		{
			logger.LogError("This brand form does not exist.({id})", command.Id);
			throw new Exception($"This brand form does not exist.({command.Id})");
		}

		if (form.State != BrandFormState.Processing)
		{
			logger.LogError("The brand form status is not processing.({id})", command.Id);
			throw new Exception($"The brand form status is not processing.({command.Id})");
		}

		try
		{
			var filterId = await playerFormRepository.BrandGetFirstIdAsync(
				expansionFormId: form.Id,
				state: PlayerFormState.Queued,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? 0;

			if (filterId > 0)
			{
				filterId--;
				await service.ExecuteAsync(
					command: new BrandFormProcessingPublishCommand(
						ExpansionFormId: form.Id,
						FilterId: filterId),
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			await brandFormRepository.UpdateCompleteAsync(
				id: command.Id,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unknown error occurred.({id})", command.Id);

			await brandFormRepository.UpdateFailedAsync(
				id: command.Id,
				status: ex.Message,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
