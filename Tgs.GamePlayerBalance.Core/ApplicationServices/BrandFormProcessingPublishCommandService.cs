using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormProcessingPublishCommandService(
	ILogger<BrandFormProcessingPublishCommandService> logger,
	IPlayerFormRepository playerFormRepository,
	IMessageSender messageSender) : ICommandService<BrandFormProcessingPublishCommand>
{
	public async ValueTask ExecuteAsync(
		BrandFormProcessingPublishCommand command,
		CancellationToken cancellationToken = default)
	{
		var pageSize = 50;
		var id = command.FilterId;

		while (id > 0)
		{
			var formIds = await playerFormRepository.BrandQueryIdAsync(
				expansionFormId: command.ExpansionFormId,
				state: PlayerFormState.Queued,
				id: id,
				pageSize: pageSize,
				cancellationToken: cancellationToken)
				.ToArrayAsync(
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<long>();

			if (!formIds.Any())
			{
				break;
			}

			foreach (var formId in formIds)
			{
				id = formId;
				try
				{
					var data = new PlayerFormModelRequest
					{
						Id = formId
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
					logger.LogError(ex, "An exception occurred when sending the player application form.({id})", formId);
				}
			}
		}
	}
}
