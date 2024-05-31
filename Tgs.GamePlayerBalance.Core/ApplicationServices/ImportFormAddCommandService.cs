using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class ImportFormAddCommandService(
	ILogger<ImportFormAddCommandService> logger,
	IImportFormRepository importFormRepository,
	IGameProviderRepository gameProviderRepository,
	IMessageSender messageSender) : ICommandService<ImportFormAddCommand>
{
	public async ValueTask ExecuteAsync(ImportFormAddCommand command, CancellationToken cancellationToken = default)
	{
		var gameProviderInfo = await gameProviderRepository.GetAsync(
			code: command.GameProviderCode,
			cancellationToken: cancellationToken)
		   .ConfigureAwait(false);

		var id = await importFormRepository.AddAsync(
			gameProviderCode: gameProviderInfo.GameProviderCode,
			players: command.Players,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var data = new ImportFormModelRequest
		{
			GameProviderCode = gameProviderInfo.GameProviderCode,
			FormId = id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.ImportFormQueued,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.ImportFormQueued,
			data);
	}
}
