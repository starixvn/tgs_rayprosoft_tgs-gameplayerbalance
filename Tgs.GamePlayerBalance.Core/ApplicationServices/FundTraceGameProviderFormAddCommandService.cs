using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundTraceGameProviderFormAddCommandService(
	ILogger<FundTraceGameProviderFormAddCommandService> logger,
	IGameProviderRepository gameProviderRepository,
	IFundTraceGameProviderFormRepository gameProviderTimeFormRepository,
	IGameFundTransferTraceRepository gameFundTransferTraceRepository,
	IMessageSender messageSender) : ICommandService<FundTraceGameProviderFormAddCommand>
{
	public async ValueTask ExecuteAsync(FundTraceGameProviderFormAddCommand command, CancellationToken cancellationToken = default)
	{
		var gameProviderInfo = await gameProviderRepository.GetAsync(
			code: command.GameProviderCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var playerCodes = await gameFundTransferTraceRepository.QueryPlayerCodeAsync(
			gameProviderCode: command.GameProviderCode,
			brandCode: null,
			startTime: command.StartTime,
			endTime: command.EndTime,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var id = await gameProviderTimeFormRepository.AddAsync(
			gameProviderCode: gameProviderInfo.GameProviderCode,
			players: playerCodes,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var data = new GameProviderTimeFormModelRequest
		{
			GameProviderCode = gameProviderInfo.GameProviderCode,
			FormId = id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.GameProviderTimeFormQueued,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.GameProviderTimeFormQueued,
			data);
	}
}
