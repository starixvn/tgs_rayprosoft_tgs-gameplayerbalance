using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class ImportFormProcessingCommandService(
	ILogger<ImportFormProcessingCommandService> logger,
	IImportFormRepository importFormRepository,
	IPlayerFormRepository playerFormRepository,
	ICommandService<ImportFormProcessingPublishCommand> service) : ICommandService<ImportFormProcessingCommand>
{
	public async ValueTask ExecuteAsync(ImportFormProcessingCommand command, CancellationToken cancellationToken = default)
	{
		var form = await importFormRepository.GetAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (form == null)
		{
			logger.LogError("This brand form does not exist.({id})", command.Id);
			throw new Exception($"This brand form does not exist.({command.Id})");
		}

		if (form.State != ImportFormState.Processing)
		{
			logger.LogError("The brand form status is not processing.({id})", command.Id);
			throw new Exception($"The brand form status is not processing.({command.Id})");
		}

		try
		{
			var filterId = await playerFormRepository.ImportGetFirstIdAsync(
				expansionFormId: form.Id,
				state: PlayerFormState.Queued,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? 0;

			if (filterId > 0)
			{
				filterId--;
				await service.ExecuteAsync(
					command: new ImportFormProcessingPublishCommand(
						ExpansionFormId: form.Id,
						FilterId: filterId),
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			await importFormRepository.UpdateCompleteAsync(
				id: command.Id,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unknown error occurred.({id})", command.Id);

			await importFormRepository.UpdateFailedAsync(
				id: command.Id,
				status: ex.Message,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
