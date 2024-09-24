using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundTraceFormQueuedCommandService(
	ILogger<FundTraceFormQueuedCommandService> logger,
	IPlayerRepository playerRepository,
	IBrandFormRepository brandFormRepository,
	IPlayerFormRepository playerFormRepository,
	IGameFundTransferTraceRepository gameFundTransferTraceRepository,
	ISnowflakeWork snowflake,
	IMessageSender messageSender) : ICommandService<FundTraceFormQueuedCommand>
{
	public async ValueTask ExecuteAsync(
		FundTraceFormQueuedCommand command,
		CancellationToken cancellationToken = default)
	{
		var form = await brandFormRepository.GetAsync(
			id: command.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (form == null)
		{
			logger.LogError("This fund trace form does not exist.({id})", command.Id);
			throw new Exception($"This fund trace form does not exist.({command.Id})");
		}

		if (form.State != BrandFormState.Queued)
		{
			logger.LogError("The fund trace form status is not queued.({id})", command.Id);
			throw new Exception($"The fund trace form status is not queued.({command.Id})");
		}

		await brandFormRepository.UpdateProcessingAsync(
			id: form.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var playerCodes = await gameFundTransferTraceRepository.QueryPlayerCodeAsync(
			gameProviderCode: form.GameProviderCode,
			brandCode: form.BrandCode is { Length: > 0 } ? form.BrandCode : null,
			startTime: form.StartTime > DateTimeOffset.UnixEpoch ? form.StartTime : null,
			endTime: form.EndTime > DateTimeOffset.UnixEpoch ? form.EndTime : null,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var playerFormAdds = new List<PlayerFormAdd>();

		foreach (var playerCode in playerCodes)
		{
			var playerInfo = await playerRepository.GetAsync(
				playerCode: playerCode,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (playerInfo == null)
			{
				logger.LogError("This player does not exist.({playerCode})", playerCode);
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
				var formIds = await playerFormRepository.FundTracePlayerFormAddAsync(
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

		var data = new FundTraceFormModelRequest
		{
			GameProviderCode = form.GameProviderCode,
			BrandCode = form.BrandCode ?? string.Empty,
			FormId = form.Id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.FundTraceFormProcessing,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.FundTraceFormProcessing,
			data);
	}
}
