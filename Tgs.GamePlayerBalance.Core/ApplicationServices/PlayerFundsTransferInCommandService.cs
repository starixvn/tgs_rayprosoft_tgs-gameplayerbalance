using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFundsTransferInCommandService : ICommandService<PlayerFundsTransferInCommand>
{
    private readonly IGameCommandClient _client;
    private readonly IGameProviderRepository _gameProviderRepository;
    private readonly ILogger<PlayerFundsTransferInCommandService> _logger;
    private readonly IGetService<FundsTransferInQuery, FundsTransferInResult> _service;

    public PlayerFundsTransferInCommandService(
        ILogger<PlayerFundsTransferInCommandService> logger,
        IGameCommandClient client,
        IGameProviderRepository gameProviderRepository,
        IGetService<FundsTransferInQuery, FundsTransferInResult> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameProviderRepository = gameProviderRepository ?? throw new ArgumentNullException(nameof(gameProviderRepository));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PlayerFundsTransferInCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _gameProviderRepository.GetIdAsync(
            code: command.GameProviderCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var player = await _client.PlayerGetAsync(
            playerCode: command.PlayerCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (player == null)
        {
            _logger.LogWarning("This player does not exist.({playerCode})", command.PlayerCode);
            return;
        }

        var query = new FundsTransferInQuery(
            gameProviderId,
            command.GameProviderCode,
            BrandCode: player.BrandCode,
            PlayerCode: player.PlayerCode,
            WalletId: player.DefaultWalletId,
            CurrencyCode: player.DefaultWalletCurrencyCode);
        _ = await _service.GetAsync(
            query: query,
            cancellationToken: cancellationToken)
           .ConfigureAwait(false);
    }
}
