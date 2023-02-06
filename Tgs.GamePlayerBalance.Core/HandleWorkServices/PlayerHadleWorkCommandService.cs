using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.HandleWorkServices;

internal class PlayerHadleWorkCommandService : ICommandService<PlayerHadleWorkCommand>
{
    private readonly IGameCommandClient _client;
    private readonly ILogger<AwcAelottoHadleWorkCommandService> _logger;
    private readonly ICommandService<PlayerCommand> _service;

    public PlayerHadleWorkCommandService(
        ILogger<AwcAelottoHadleWorkCommandService> logger,
        IGameCommandClient client,
        ICommandService<PlayerCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PlayerHadleWorkCommand command, CancellationToken cancellationToken = default)
    {
        var player = await _client
            .PlayerGetAsync(
                playerCode: command.PlayerCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
        {
            _logger.LogError("This player does not exist.({playerCode})", command.PlayerCode);
        }

        try
        {
            var playerCommand = new PlayerCommand(
                command.GameProviderCode,
                player.BrandCode,
                player.PlayerCode,
                player.DefaultWalletId,
                player.DefaultWalletCurrencyCode);

            await _service
                 .ExecuteAsync(
                     command: playerCommand,
                     cancellationToken: cancellationToken)
                 .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The player's balance withdrawal process failed.({playerCode})", player.PlayerCode);
        }
    }
}
