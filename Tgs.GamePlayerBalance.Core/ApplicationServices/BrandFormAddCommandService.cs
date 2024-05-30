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
		var gameProviderId = await gameProviderRepository.GetIdAsync(
			code: command.GameProviderCode,
			cancellationToken: cancellationToken)
		   .ConfigureAwait(false);

		var brandId = await brandRepository.GetIdAsync(
			code: command.BrandCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var id = await brandFormRepository.AddAsync(
			gameProviderId: gameProviderId,
			gameProviderCode: command.GameProviderCode,
			brandId: brandId,
			brandCode: command.BrandCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var subject = GamePlayerBalanceNatsConst.BrandFormQueued;
		if (command.GameProviderCode.StartsWith("AWC_", StringComparison.OrdinalIgnoreCase))
		{
			subject = $"{subject}.{command.GameProviderCode.Replace("_", ".").ToLower()}";
		}

		var data = new BrandFormModelRequest
		{
			Id = id
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
