using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFormAddCommandService : ICommandService<PlayerFormAddCommand>
{
    private readonly IGameCommandClient _client;
    private readonly IGameProviderRepository _gameProviderRepository;
    private readonly ILogger<PlayerFormAddCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IPlayerFormRepository _playerFormRepository;

    public PlayerFormAddCommandService(
        ILogger<PlayerFormAddCommandService> logger,
        IGameCommandClient client,
        IGameProviderRepository gameProviderRepository,
        IPlayerFormRepository playerFormRepository,
        IMessageSender messageSender)
    {
        _gameProviderRepository = gameProviderRepository ?? throw new ArgumentNullException(nameof(gameProviderRepository));
        _playerFormRepository = playerFormRepository ?? throw new ArgumentNullException(nameof(playerFormRepository));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(PlayerFormAddCommand command, CancellationToken cancellationToken = default)
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
            _logger.LogError("This player does not exist.({playerCode})", command.PlayerCode);
            throw new Exception($"This player does not exist.({command.PlayerCode})");
        }

        var id = await _playerFormRepository
            .AddAsync(
                gameProviderId,
                command.GameProviderCode,
                brandId: player.BrandId,
                brandCode: player.BrandCode,
                playerCode: player.PlayerCode,
                username: player.Username,
                walletId: player.DefaultWalletId,
                currencyCode: player.DefaultWalletCurrencyCode,
                cancellationToken: cancellationToken);

        var request = new PlayerFormModelRequest
        {
            Id = id
        };

        await _messageSender
            .PublishAsync(
                subject: "game.playerbalance.player.form.queued",
                data: request.ToByteArray(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);
    }
}
