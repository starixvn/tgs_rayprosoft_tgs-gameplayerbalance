using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormAddCommandService(
	ILogger<BrandFormAddCommandService> logger,
	IBrandRepository brandRepository,
	IGameProviderRepository gameProviderRepository,
	IBrandFormRepository brandFormRepository,
	IMessageSender messageSender) : ICommandService<BrandFormAddCommand>
{
	public async ValueTask ExecuteAsync(BrandFormAddCommand command, CancellationToken cancellationToken = default)
	{
		var brandInfo = await brandRepository.GetAsync(
			code: command.BrandCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var gameProviderInfo = await gameProviderRepository.GetAsync(
			code: command.GameProviderCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var id = await brandFormRepository.AddAsync(
			gameProviderCode: gameProviderInfo.GameProviderCode,
			brandCode: brandInfo.BrandCode,
			startTime: DateTimeOffset.UnixEpoch,
			endTime: DateTimeOffset.UnixEpoch,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var subject = GamePlayerBalanceNatsConst.BrandFormQueued;
		if (gameProviderInfo.GameProviderCode.StartsWith("AWC_", StringComparison.OrdinalIgnoreCase))
		{
			subject = $"{subject}.{gameProviderInfo.GameProviderCode.Replace("_", ".").ToLower()}";
		}

		var data = new BrandFormModelRequest
		{
			GameProviderCode = gameProviderInfo.GameProviderCode,
			BrandCode = brandInfo.BrandCode,
			FormId = id
		};

		await messageSender.PublishAsync(
			subject: subject,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			subject,
			data);
	}
}
