using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFundsTransferInCommandService(
	ILogger<PlayerFundsTransferInCommandService> logger,
	IGameCommandClient client,
	IGameProviderRepository gameProviderRepository,
	IGetService<FundsTransferInQuery, FundsTransferInResult> service) : ICommandService<PlayerFundsTransferInCommand>
{
	public async ValueTask ExecuteAsync(PlayerFundsTransferInCommand command, CancellationToken cancellationToken = default)
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
			logger.LogWarning("This player does not exist.({playerCode})", command.PlayerCode);
			return;
		}

		var query = new FundsTransferInQuery(
			gameProviderId,
			command.GameProviderCode,
			BrandCode: player.BrandCode,
			PlayerCode: player.PlayerCode,
			WalletId: player.DefaultWalletId,
			CurrencyCode: player.DefaultWalletCurrencyCode);
		_ = await service.GetAsync(
			query: query,
			cancellationToken: cancellationToken)
		   .ConfigureAwait(false);
	}
}
