using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFormAddCommandService(
	ILogger<PlayerFormAddCommandService> logger,
	IPlayerRepository playerRepository,
	IPlayerFormRepository playerFormRepository,
	IMessageSender messageSender) : ICommandService<PlayerFormAddCommand>
{
	public async ValueTask ExecuteAsync(PlayerFormAddCommand command, CancellationToken cancellationToken = default)
	{
		var player = await playerRepository.GetAsync(
			playerCode: command.PlayerCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? throw new ArgumentNullException(command.PlayerCode, "This player does not exist.");

		var id = await playerFormRepository.AddAsync(
			gameProviderCode: command.GameProviderCode,
			brandCode: player.BrandCode,
			playerCode: player.PlayerCode,
			username: player.Username,
			walletId: player.WalletId,
			currencyCode: player.CurrencyCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var data = new PlayerFormModelRequest
		{
			GameProviderCode = command.GameProviderCode,
			BrandCode = player.BrandCode,
			PlayerCode = player.PlayerCode,
			FormId = id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.PlayerFormQueued,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.PlayerFormQueued,
			data);
	}
}
