using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GameProviders.Client;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormByAwcAelottoQueuedCommandService : ICommandService<BrandFormByAwcAelottoQueuedCommand>
{
    private readonly IBrandFormRepository _brandFormRepository;
    private readonly IGameCommandClient _gameCommandClient;
    private readonly IGameProvidersBalancesClient _gameProvidersBalancesClient;
    private readonly ILogger<BrandFormByAwcAelottoQueuedCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IPlayerFormRepository _playerFormRepository;
    private readonly ISnowflake _snowflake;

    public BrandFormByAwcAelottoQueuedCommandService(
        ILogger<BrandFormByAwcAelottoQueuedCommandService> logger,
        IGameProvidersBalancesClient gameProvidersBalancesClient,
        IGameCommandClient gameCommandClient,
        IBrandFormRepository brandFormRepository,
        IPlayerFormRepository playerFormRepository,
        ISnowflake snowflake,
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
        BrandFormByAwcAelottoQueuedCommand command,
        CancellationToken cancellationToken = default)
    {
        var form = await _brandFormRepository
            .GetAsync(
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

        await _brandFormRepository
            .UpdateProcessingAsync(
                id: command.Id,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var balanceResponses = _gameProvidersBalancesClient
            .AwcAelottoGetBalances(
                brandCode: form.BrandCode,
                cancellationToken: cancellationToken)
            .ToEnumerable()
            ?? Enumerable.Empty<BalanceResponse>();

        var playerFormAdds = new List<PlayerFormAdd>();
        foreach (var balanceResponse in balanceResponses)
        {
            var player = await _gameCommandClient
                 .PlayerGetAsync(
                  playerCode: balanceResponse.PlayerCode,
                  cancellationToken: cancellationToken)
                 .ConfigureAwait(false);

            if (player is null)
            {
                _logger.LogError("This player does not exist.({playerCode})", balanceResponse.PlayerCode);
                continue;
            }
            playerFormAdds.Add(new PlayerFormAdd(
                FormId: _snowflake.NextId(),
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

        try
        {
            if (playerFormAdds.Any())
            {
                _ = await _playerFormRepository
                    .BrandPlayerFormAddAsync(
                        playerFormAdds,
                        cancellationToken)
                    .ToArrayAsync(
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unknown error occurred.({id})", command.Id);
        }

        var request = new BrandFormModelRequest
        {
            Id = form.Id
        };

        var subject = "game.playerbalance.brand.form.processing";

        await _messageSender
            .PublishAsync(
                subject: subject,
                data: request.ToByteArray(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("LogOn:{logOn}  Subject:{subject} Information Request:{receive}",
            DateTime.Now.ToString("o"),
            subject,
            request);
    }
}
