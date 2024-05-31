using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormByAwcAelottoQueuedCommandService(
	ILogger<BrandFormByAwcAelottoQueuedCommandService> logger,
	IGameProvidersBalancesClient gameProvidersBalancesClient,
	IBrandFormRepository brandFormRepository,
	IPlayerRepository playerRepository,
	IPlayerFormRepository playerFormRepository,
	ISnowflakeWork snowflake,
	IMessageSender messageSender) : ICommandService<BrandFormByAwcAelottoQueuedCommand>
{
	public async ValueTask ExecuteAsync(
		BrandFormByAwcAelottoQueuedCommand command,
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
			id: form.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var balanceResponses = await gameProvidersBalancesClient.AwcAelottoGetBalances(
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
				var player = await playerRepository.GetAsync(
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
					GameProviderCode: form.GameProviderCode,
					BrandCode: form.BrandCode,
					PlayerCode: player.PlayerCode,
					Username: player.Username,
					WalletId: player.WalletId,
					CurrencyCode: player.CurrencyCode));
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
			GameProviderCode = form.GameProviderCode,
			BrandCode = form.BrandCode,
			FormId = form.Id
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
