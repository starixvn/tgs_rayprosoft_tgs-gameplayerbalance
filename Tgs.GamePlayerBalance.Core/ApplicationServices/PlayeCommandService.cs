using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayeCommandService : ICommandService<PlayerCommand>
{
    private readonly IGameProvidersClient _client;
    private readonly IGameProviderRepository _gameProviderRepository;
    private readonly ILogger<PlayeCommandService> _logger;
    private readonly ICommandService<FundsTransferInCommand> _service;

    public PlayeCommandService(
        ILogger<PlayeCommandService> logger,
        IGameProvidersClient client,
        IGameProviderRepository gameProviderRepository,
        ICommandService<FundsTransferInCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameProviderRepository = gameProviderRepository ?? throw new ArgumentNullException(nameof(gameProviderRepository));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PlayerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var gameProviderId = await _gameProviderRepository
                .GetIdAsync(
                    command.GameProviderCode,
                    cancellationToken)
                .ConfigureAwait(false);

            var balance = await _client
                .PlayerBalanceAsync(
                    gameProviderCode: command.GameProviderCode,
                    playerCode: command.PlayerCode)
                .ConfigureAwait(false);
            if (balance > 0)
            {
                var fundsTransferInCommand = new FundsTransferInCommand(
                    gameProviderId,
                    command.GameProviderCode,
                    BrandCode: command.BrandCode,
                    PlayerCode: command.PlayerCode,
                    WalletId: command.WalletId,
                    CurrencyCode: command.CurrencyCode);

                await _service
                     .ExecuteAsync(
                         command: fundsTransferInCommand,
                         cancellationToken: cancellationToken)
                     .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The player's balance withdrawal process failed.({playerCode})", command.PlayerCode);
        }
    }
}
