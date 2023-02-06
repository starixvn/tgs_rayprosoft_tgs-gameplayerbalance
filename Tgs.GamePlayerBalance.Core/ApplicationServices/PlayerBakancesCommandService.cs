using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerBakancesCommandService : ICommandService<PlayerBakancesCommand>
{
    private readonly IGameProvidersClient _client;
    private readonly ILogger<PlayerBakancesCommandService> _logger;
    private readonly ICommandService<FundsTransferInCommand> _service;

    public PlayerBakancesCommandService(
        ILogger<PlayerBakancesCommandService> logger,
        IGameProvidersClient client,
        ICommandService<FundsTransferInCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PlayerBakancesCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.PlayerInfos.Any())
        {
            return;
        }

        foreach (var playerInfo in command.PlayerInfos)
        {
            try
            {
                var balance = await _client
                    .PlayerBalanceAsync(
                        gameProviderCode: command.GameProviderCode,
                        playerCode: playerInfo.PlayerCode)
                    .ConfigureAwait(false);
                if (balance > 0)
                {
                    var fundsTransferInCommand = new FundsTransferInCommand(
                        command.GameProviderId,
                        command.GameProviderCode,
                        BrandCode: playerInfo.BrandCode,
                        PlayerCode: playerInfo.PlayerCode,
                        WalletId: playerInfo.DefaultWalletId,
                        CurrencyCode: playerInfo.DefaultWalletCurrencyCode);

                    await _service
                         .ExecuteAsync(
                             command: fundsTransferInCommand,
                             cancellationToken: cancellationToken)
                         .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The player's balance withdrawal process failed.({playerCode})", playerInfo.PlayerCode);
            }
        }
    }
}
