using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandTimeFormAddCommandService(
	ILogger<BrandTimeFormAddCommandService> logger,
	IBrandRepository brandRepository,
	IGameProviderRepository gameProviderRepository,
	IBrandTimeFormRepository brandTimeFormRepository,
	IGameFundTransferTraceRepository gameFundTransferTraceRepository,
	IMessageSender messageSender) : ICommandService<BrandTimeFormAddCommand>
{
	public async ValueTask ExecuteAsync(BrandTimeFormAddCommand command, CancellationToken cancellationToken = default)
	{
		var brandInfo = await brandRepository.GetAsync(
			code: command.BrandCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var gameProviderInfo = await gameProviderRepository.GetAsync(
			code: command.GameProviderCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var playerCodes = await gameFundTransferTraceRepository.QueryPlayerCodeAsync(
			gameProviderCode: command.GameProviderCode,
			brandCode: command.BrandCode,
			startTime: command.StartTime,
			endTime: command.EndTime,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var id = await brandTimeFormRepository.AddAsync(
			gameProviderCode: gameProviderInfo.GameProviderCode,
			brandCode: brandInfo.BrandCode,
			players: playerCodes,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var data = new BrandTimeFormModelRequest
		{
			GameProviderCode = gameProviderInfo.GameProviderCode,
			BrandCode = brandInfo.BrandCode,
			FormId = id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.BrandTimeFormQueued,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.BrandTimeFormQueued,
			data);
	}
}
