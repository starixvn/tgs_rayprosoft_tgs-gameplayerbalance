using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerBakanceCommandService : ICommandService<PlayerBakanceCommand>
{
    private readonly IGameCommandClient _client;
    private readonly IGameProviderRepository _gameProviderRepository;
    private readonly ILogger<PlayerBakanceCommandService> _logger;
    private readonly ICommandService<FundsTransferInCommand> _service;

    public PlayerBakanceCommandService(
        ILogger<PlayerBakanceCommandService> logger,
        IGameCommandClient client,
        IGameProviderRepository gameProviderRepository,
        ICommandService<FundsTransferInCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameProviderRepository = gameProviderRepository ?? throw new ArgumentNullException(nameof(gameProviderRepository));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PlayerBakanceCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _gameProviderRepository
            .GetIdAsync(
                code: command.GameProviderCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var player = await _client
            .PlayerGetAsync(
                playerCode: command.PlayerCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (player == null)
        {
            _logger.LogWarning("This player does not exist.({playerCode})", command.PlayerCode);
            return;
        }

        var fundsTransferInCommand = new FundsTransferInCommand(
            gameProviderId,
            command.GameProviderCode,
            BrandCode: player.BrandCode,
            PlayerCode: player.PlayerCode,
            WalletId: player.DefaultWalletId,
            CurrencyCode: player.DefaultWalletCurrencyCode);
        await _service
             .ExecuteAsync(
                 command: fundsTransferInCommand,
                 cancellationToken: cancellationToken)
             .ConfigureAwait(false);
    }
}
