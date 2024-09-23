using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundTraceFormProcessingCommandService(
	ILogger<FundTraceFormProcessingCommandService> logger,
	IFundTraceFormRepository fundTraceFormRepository,
	IPlayerFormRepository playerFormRepository,
	ICommandService<FundTraceFormProcessingPublishCommand> service) : ICommandService<FundTraceFormProcessingCommand>
{
	public async ValueTask ExecuteAsync(
		FundTraceFormProcessingCommand command,
		CancellationToken cancellationToken = default)
	{
		var form = await fundTraceFormRepository.GetAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (form == null)
		{
			logger.LogError("This fund trace form does not exist.({id})", command.Id);
			throw new Exception($"This fund trace form does not exist.({command.Id})");
		}

		if (form.State != FundTraceFormState.Processing)
		{
			logger.LogError("The fund trace form status is not processing.({id})", command.Id);
			throw new Exception($"The fund trace form status is not processing.({command.Id})");
		}

		try
		{
			var filterId = await playerFormRepository.FundTraceGetFirstIdAsync(
				expansionFormId: form.Id,
				state: PlayerFormState.Queued,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? 0;

			if (filterId > 0)
			{
				filterId--;
				await service.ExecuteAsync(
					command: new FundTraceFormProcessingPublishCommand(
						ExpansionFormId: form.Id,
						FilterId: filterId),
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			await fundTraceFormRepository.UpdateCompleteAsync(
				id: command.Id,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unknown error occurred.({id})", command.Id);

			await fundTraceFormRepository.UpdateFailedAsync(
				id: command.Id,
				status: ex.Message,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
