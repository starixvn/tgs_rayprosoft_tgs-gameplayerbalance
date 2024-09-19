using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class GameProviderTimeFormQueuedCommandService(
	ILogger<GameProviderTimeFormQueuedCommandService> logger,
	IPlayerRepository playerRepository,
	IFundTraceGameProviderFormRepository gameProviderTimeFormRepository,
	IPlayerFormRepository playerFormRepository,
	ISnowflakeWork snowflake,
	IMessageSender messageSender) : ICommandService<GameProviderTimeFormQueuedCommand>
{
	public async ValueTask ExecuteAsync(
		GameProviderTimeFormQueuedCommand command,
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

		if (form.State != GameProviderTimeFormState.Queued)
		{
			logger.LogError("The game provider time form status is not queued.({id})", command.Id);
			throw new Exception($"The game provider time form status is not queued.({command.Id})");
		}

		await gameProviderTimeFormRepository.UpdateProcessingAsync(
			id: form.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var playerFormAdds = new List<PlayerFormAdd>();

		foreach (var player in form.Players)
		{
			var playerInfo = await playerRepository.GetAsync(
				playerCode: player.PlayerCode,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (playerInfo == null)
			{
				logger.LogError("This player does not exist.({playerCode})", player.PlayerCode);
				continue;
			}

			playerFormAdds.Add(new PlayerFormAdd(
				FormId: snowflake.GetId(),
				ExpansionFormId: form.Id,
				GameProviderCode: form.GameProviderCode,
				BrandCode: playerInfo.BrandCode,
				PlayerCode: playerInfo.PlayerCode,
				Username: playerInfo.Username,
				WalletId: playerInfo.WalletId,
				CurrencyCode: playerInfo.CurrencyCode));
		}

		try
		{
			if (playerFormAdds.Count != 0)
			{
				var formIds = await playerFormRepository.GameProviderTimePlayerFormAddAsync(
					playerFormAdds,
					cancellationToken)
					.ToArrayAsync(
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unknown error occurred.({id})", form.Id);
		}

		var data = new GameProviderTimeFormModelRequest
		{
			FormId = form.Id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.GameProviderTimeFormProcessing,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.GameProviderTimeFormProcessing,
			data);
	}
}
