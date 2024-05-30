using Google.Protobuf;
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
		var gameProviderId = await gameProviderRepository.GetIdAsync(
			code: command.GameProviderCode,
			cancellationToken: cancellationToken)
		   .ConfigureAwait(false);

		var id = await importFormRepository.AddAsync(
			gameProviderId: gameProviderId,
			gameProviderCode: command.GameProviderCode,
			players: command.Players,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
		var data = new ImportFormModelRequest
		{
			Id = id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.ImportFormQueued,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogDebug("LogOn:{logOn}  Subject:{subject} Information Request:{receive}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.ImportFormQueued,
			data);
	}
}
