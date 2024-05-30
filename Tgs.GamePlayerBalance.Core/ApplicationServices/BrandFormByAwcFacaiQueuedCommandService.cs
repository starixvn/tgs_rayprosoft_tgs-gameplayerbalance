using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormByAwcFacaiQueuedCommandService(
	ILogger<BrandFormByAwcFacaiQueuedCommandService> logger,
	IGameProvidersBalancesClient gameProvidersBalancesClient,
	IGameCommandClient gameCommandClient,
	IBrandFormRepository brandFormRepository,
	IPlayerFormRepository playerFormRepository,
	ISnowflakeWork snowflake,
	IMessageSender messageSender) : ICommandService<BrandFormByAwcFacaiQueuedCommand>
{
	public async ValueTask ExecuteAsync(
		BrandFormByAwcFacaiQueuedCommand command,
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

		if (form.State != BrandFormState.Queued)
		{
			logger.LogError("The brand form status is not queued.({id})", command.Id);
			throw new Exception($"The brand form status is not queued.({command.Id})");
		}

		await brandFormRepository.UpdateProcessingAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var balanceResponses = await gameProvidersBalancesClient.AwcFacaiGetBalances(
			brandCode: form.BrandCode,
			cancellationToken: cancellationToken)
			.ToArrayAsync(
			cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? Enumerable.Empty<BalanceResponse>();

		var pageSize = 500;
		var tempbalanceResponses = balanceResponses.Take(pageSize);

		if (tempbalanceResponses.Any())
		{
			var playerFormAdds = new List<PlayerFormAdd>();
			foreach (var balanceResponse in tempbalanceResponses)
			{
				var player = await gameCommandClient.PlayerGetAsync(
					playerCode: balanceResponse.PlayerCode,
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				if (player is null)
				{
					logger.LogError("This player does not exist.({playerCode})", balanceResponse.PlayerCode);
					continue;
				}

				playerFormAdds.Add(new PlayerFormAdd(
					FormId: snowflake.GetId(),
					ExpansionFormId: form.Id,
					GameProviderId: form.GameProviderId,
					GameProviderCode: form.GameProviderCode,
					BrandId: form.BrandId,
					BrandCode: form.BrandCode,
					PlayerCode: player.PlayerCode,
					Username: player.Username,
					WalletId: player.DefaultWalletId,
					CurrencyCode: player.DefaultWalletCurrencyCode));
			}

			if (playerFormAdds.Count != 0)
			{
				try
				{
					_ = await playerFormRepository.BrandPlayerFormAddAsync(
						source: playerFormAdds,
						cancellationToken: cancellationToken)
						.ToArrayAsync(
						cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "An unknown error occurred.({id})", command.Id);
				}
			}
		}

		var data = new BrandFormModelRequest
		{
			Id = form.Id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.BrandFormProcessing,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.BrandFormProcessing,
			data);
	}
}
