using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormQueuedCommandService(
	ILogger<BrandFormQueuedCommandService> logger,
	IBrandRepository brandRepository,
	IPlayerRepository playerRepository,
	IBrandFormRepository brandFormRepository,
	IPlayerFormRepository playerFormRepository,
	ISnowflakeWork snowflake,
	IMessageSender messageSender) : ICommandService<BrandFormQueuedCommand>
{
	public async ValueTask ExecuteAsync(
		BrandFormQueuedCommand command,
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

		var brandInfo = await brandRepository.GetAsync(form.BrandCode, cancellationToken).ConfigureAwait(false);

		await brandFormRepository.UpdateProcessingAsync(
			id: form.Id,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var pageIndex = 1;
		var pageSize = 1000;

		while (true)
		{
			var playerInfos = await playerRepository.QueryAsync(
				brandId: brandInfo.BrandId,
				pageIndex: pageIndex,
				pageSize: pageSize,
				cancellationToken: cancellationToken)
				.ToArrayAsync(
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? Enumerable.Empty<PlayerInfo>();

			if (!playerInfos.Any())
			{
				break;
			}

			pageIndex++;

			try
			{
				var playerFormAdds = new List<PlayerFormAdd>();

				foreach (var playerInfo in playerInfos)
				{
					playerFormAdds.Add(new PlayerFormAdd(
						FormId: snowflake.GetId(),
						ExpansionFormId: form.Id,
						GameProviderCode: form.GameProviderCode,
						BrandCode: form.BrandCode,
						PlayerCode: playerInfo.PlayerCode,
						Username: playerInfo.Username,
						WalletId: playerInfo.WalletId,
						CurrencyCode: playerInfo.CurrencyCode));
				}

				var formIds = await playerFormRepository.BrandPlayerFormAddAsync(
					source: playerFormAdds,
					cancellationToken: cancellationToken)
					.ToArrayAsync(
					cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An unknown error occurred.({id})", form.Id);
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
