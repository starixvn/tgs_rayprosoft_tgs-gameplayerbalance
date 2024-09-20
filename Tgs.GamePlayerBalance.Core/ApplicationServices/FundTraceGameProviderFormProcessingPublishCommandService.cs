using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundTraceGameProviderFormProcessingPublishCommandService(
	ILogger<FundTraceGameProviderFormProcessingPublishCommandService> logger,
	IPlayerFormRepository playerFormRepository,
	IMessageSender messageSender) : ICommandService<FundTraceGameProviderFormProcessingPublishCommand>
{
	public async ValueTask ExecuteAsync(
		FundTraceGameProviderFormProcessingPublishCommand command,
		CancellationToken cancellationToken = default)
	{
		var pageSize = 50;
		var id = command.FilterId;

		while (id > 0)
		{
			var playerForms = await playerFormRepository.FundTraceGameProviderQueryAsync(
				expansionFormId: command.ExpansionFormId,
				state: PlayerFormState.Queued,
				id: id,
				pageSize: pageSize,
				cancellationToken: cancellationToken)
				.ToArrayAsync(
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<PlayerForm>();

			if (!playerForms.Any())
			{
				break;
			}

			foreach (var playerForm in playerForms)
			{
				id = playerForm.Id;
				try
				{
					var data = new PlayerFormModelRequest
					{
						GameProviderCode = playerForm.GameProviderCode,
						BrandCode = playerForm.BrandCode,
						PlayerCode = playerForm.PlayerCode,
						FormId = playerForm.Id
					};

					await messageSender.PublishAsync(
						subject: GamePlayerBalanceNatsConst.PlayerFormQueued,
						data: data,
						cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
						DateTime.Now.ToString("o"),
						GamePlayerBalanceNatsConst.PlayerFormQueued,
						data);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "An exception occurred when sending the player application form.({id})", id);
				}
			}
		}
	}
}
