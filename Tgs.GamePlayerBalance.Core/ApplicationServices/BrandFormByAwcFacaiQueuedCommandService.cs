using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormByAwcFacaiQueuedCommandService : ICommandService<BrandFormByAwcFacaiQueuedCommand>
{
    private readonly IBrandFormRepository _brandFormRepository;
    private readonly IGameCommandClient _gameCommandClient;
    private readonly IGameProvidersBalancesClient _gameProvidersBalancesClient;
    private readonly ILogger<BrandFormByAwcFacaiQueuedCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IPlayerFormRepository _playerFormRepository;
    private readonly ISnowflakeWork _snowflake;

    public BrandFormByAwcFacaiQueuedCommandService(
        ILogger<BrandFormByAwcFacaiQueuedCommandService> logger,
        IGameProvidersBalancesClient gameProvidersBalancesClient,
        IGameCommandClient gameCommandClient,
        IBrandFormRepository brandFormRepository,
        IPlayerFormRepository playerFormRepository,
        ISnowflakeWork snowflake,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameProvidersBalancesClient = gameProvidersBalancesClient ?? throw new ArgumentNullException(nameof(gameProvidersBalancesClient));
        _gameCommandClient = gameCommandClient ?? throw new ArgumentNullException(nameof(gameCommandClient));
        _brandFormRepository = brandFormRepository ?? throw new ArgumentNullException(nameof(brandFormRepository));
        _playerFormRepository = playerFormRepository ?? throw new ArgumentNullException(nameof(playerFormRepository));
        _snowflake = snowflake ?? throw new ArgumentNullException(nameof(snowflake));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask ExecuteAsync(
        BrandFormByAwcFacaiQueuedCommand command,
        CancellationToken cancellationToken = default)
    {
        var form = await _brandFormRepository.GetAsync(
            id: command.Id,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (form == null)
        {
            _logger.LogError("This brand form does not exist.({id})", command.Id);
            throw new Exception($"This brand form does not exist.({command.Id})");
        }

        if (form.State != BrandFormState.Queued)
        {
            _logger.LogError("The brand form status is not queued.({id})", command.Id);
            throw new Exception($"The brand form status is not queued.({command.Id})");
        }

        await _brandFormRepository.UpdateProcessingAsync(
            id: command.Id,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var balanceResponses = await _gameProvidersBalancesClient.AwcFacaiGetBalances(
            brandCode: form.BrandCode,
            cancellationToken: cancellationToken)
            .ToArrayAsync(
            cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<BalanceResponse>();

        var pageSize = 500;
        var tempbalanceResponses = balanceResponses.Take(pageSize);

        if (tempbalanceResponses.Any())
        {
            var playerFormAdds = new List<PlayerFormAdd>();
            foreach (var balanceResponse in tempbalanceResponses)
            {
                var player = await _gameCommandClient.PlayerGetAsync(
                    playerCode: balanceResponse.PlayerCode,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (player is null)
                {
                    _logger.LogError("This player does not exist.({playerCode})", balanceResponse.PlayerCode);
                    continue;
                }
                playerFormAdds.Add(new PlayerFormAdd(
                    FormId: _snowflake.GetId(),
                    ExpansionFormId: form.Id,
                    GameProviderId: form.GameProviderId,
                    GameProviderCode: form.GameProviderCode,
                    BrandId: form.BrandId,
                    BrandCode: form.BrandCode,
                    PlayerCode: player.PlayerCode,
                    Username: player.Username,
                    WalletId: player.DefaultWalletId,
                    CurrencyCode: player.DefaultWalletCurrencyCode));
            }
            if (playerFormAdds.Any())
            {
                try
                {
                    _ = await _playerFormRepository.BrandPlayerFormAddAsync(
                        source: playerFormAdds,
                        cancellationToken: cancellationToken)
                        .ToArrayAsync(
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unknown error occurred.({id})", command.Id);
                }
            }
        }

        var request = new BrandFormModelRequest
        {
            Id = form.Id
        };

        var subject = "game.playerbalance.brand.form.processing";

        await _messageSender.PublishAsync(
            subject: subject,
            data: request.ToByteArray(),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug("LogOn:{logOn}  Subject:{subject} Information Request:{receive}",
                DateTime.Now.ToString("o"),
                subject,
                request);
    }
}
