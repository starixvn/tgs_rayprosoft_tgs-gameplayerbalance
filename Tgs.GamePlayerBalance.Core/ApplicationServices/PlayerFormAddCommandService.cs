using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFormAddCommandService(
	ILogger<PlayerFormAddCommandService> logger,
	IGameCommandClient client,
	IGameProviderRepository gameProviderRepository,
	IPlayerFormRepository playerFormRepository,
	IMessageSender messageSender) : ICommandService<PlayerFormAddCommand>
{
	public async ValueTask ExecuteAsync(PlayerFormAddCommand command, CancellationToken cancellationToken = default)
	{
		var gameProviderId = await gameProviderRepository.GetIdAsync(
			code: command.GameProviderCode,
			cancellationToken: cancellationToken)
		   .ConfigureAwait(false);

		var player = await client.PlayerGetAsync(
			playerCode: command.PlayerCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (player == null)
		{
			logger.LogError("This player does not exist.({playerCode})", command.PlayerCode);
			throw new Exception($"This player does not exist.({command.PlayerCode})");
		}

		var id = await playerFormRepository.AddAsync(
			gameProviderId: gameProviderId,
			gameProviderCode: command.GameProviderCode,
			brandId: player.BrandId,
			brandCode: player.BrandCode,
			playerCode: player.PlayerCode,
			username: player.Username,
			walletId: player.DefaultWalletId,
			currencyCode: player.DefaultWalletCurrencyCode,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var data = new BrandFormModelRequest
		{
			Id = id
		};

		await messageSender.PublishAsync(
			subject: GamePlayerBalanceNatsConst.PlayerFormQueued,
			data: data,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("LogOn:{logOn}  Subject:{subject} Information Request:{data}",
			DateTime.Now.ToString("o"),
			GamePlayerBalanceNatsConst.PlayerFormQueued,
			data);
	}
}
