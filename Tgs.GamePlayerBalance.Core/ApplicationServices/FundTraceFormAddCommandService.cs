using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundTraceFormAddCommandService(
	ILogger<FundTraceFormAddCommandService> logger,
	IGameProviderRepository gameProviderRepository,
	IBrandRepository brandRepository,
	IBrandFormRepository brandFormRepository,
	IMessageSender messageSender) : ICommandService<FundTraceFormAddCommand>
{
	public async ValueTask ExecuteAsync(FundTraceFormAddCommand command, CancellationToken cancellationToken = default)
	{
		var brandInfo = command.BrandCode is { Length: > 0 } ? await brandRepository.GetAsync(
			code: command.BrandCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false) : null;
		var gameProviderInfo = await gameProviderRepository.GetAsync(
			code: command.GameProviderCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var id = await brandFormRepository.AddAsync(
			gameProviderCode: command.GameProviderCode,
			brandCode: command.BrandCode is { Length: > 0 } ? command.BrandCode : string.Empty,
			startTime: command.StartTime?.ToUniversalTime() ?? DateTimeOffset.UnixEpoch,
			endTime: command.EndTime?.ToUniversalTime() ?? DateTimeOffset.UnixEpoch,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var data = new FundTraceFormModelRequest
		{
			GameProviderCode = gameProviderInfo.GameProviderCode,
			BrandCode = brandInfo?.BrandCode ?? string.Empty,
			FormId = id,
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.FundTraceFormQueued,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.FundTraceFormQueued,
			data);
	}
}
