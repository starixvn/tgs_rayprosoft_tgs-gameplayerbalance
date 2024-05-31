using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class ImportFormQueuedCommandService(
	ILogger<ImportFormQueuedCommandService> logger,
	IImportFormRepository importFormRepository,
	IPlayerRepository playerRepository,
	IPlayerFormRepository playerFormRepository,
	ISnowflakeWork snowflake,
	IMessageSender messageSender) : ICommandService<ImportFormQueuedCommand>
{
	public async ValueTask ExecuteAsync(
		ImportFormQueuedCommand command,
		CancellationToken cancellationToken = default)
	{
		var form = await importFormRepository.GetAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (form == null)
		{
			logger.LogError("This import form does not exist.({id})", command.Id);
			throw new Exception($"This import form does not exist.({command.Id})");
		}

		if (form.State != ImportFormState.Queued)
		{
			logger.LogError("The import form status is not queued.({id})", command.Id);
			throw new Exception($"The import form status is not queued.({command.Id})");
		}

		await importFormRepository.UpdateProcessingAsync(
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
				var formIds = await playerFormRepository.ImportPlayerFormAddAsync(
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

		var data = new ImportFormModelRequest
		{
			FormId = form.Id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.ImportFormProcessing,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.ImportFormProcessing,
			data);
	}
}
