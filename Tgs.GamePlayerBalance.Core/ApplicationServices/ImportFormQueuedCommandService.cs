using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class ImportFormQueuedCommandService : ICommandService<ImportFormQueuedCommand>
{
    private readonly IGameCommandClient _client;
    private readonly IImportFormRepository _importFormRepository;
    private readonly ILogger<ImportFormQueuedCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IPlayerFormRepository _playerFormRepository;
    private readonly ISnowflake _snowflake;

    public ImportFormQueuedCommandService(
        ILogger<ImportFormQueuedCommandService> logger,
        IGameCommandClient client,
        IImportFormRepository importFormRepository,
        IPlayerFormRepository playerFormRepository,
        ISnowflake snowflake,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _importFormRepository = importFormRepository ?? throw new ArgumentNullException(nameof(importFormRepository));
        _playerFormRepository = playerFormRepository ?? throw new ArgumentNullException(nameof(playerFormRepository));
        _snowflake = snowflake ?? throw new ArgumentNullException(nameof(snowflake));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask ExecuteAsync(
        ImportFormQueuedCommand command,
        CancellationToken cancellationToken = default)
    {
        var form = await _importFormRepository.GetAsync(
            id: command.Id,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (form == null)
        {
            _logger.LogError("This brand form does not exist.({id})", command.Id);
            throw new Exception($"This brand form does not exist.({command.Id})");
        }

        if (form.State != ImportFormState.Queued)
        {
            _logger.LogError("The brand form status is not queued.({id})", command.Id);
            throw new Exception($"The brand form status is not queued.({command.Id})");
        }

        await _importFormRepository.UpdateProcessingAsync(
            id: command.Id,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var playerFormAdds = new List<PlayerFormAdd>();

        foreach (var player in form.Players)
        {
            var playerInfo = await _client.PlayerGetAsync(
                playerCode: player.PlayerCode,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (playerInfo == null)
            {
                continue;
            }

            playerFormAdds.Add(new PlayerFormAdd(
                FormId: _snowflake.NextId(),
                ExpansionFormId: form.Id,
                GameProviderId: form.GameProviderId,
                GameProviderCode: form.GameProviderCode,
                BrandId: playerInfo.BrandId,
                BrandCode: playerInfo.BrandCode,
                PlayerCode: playerInfo.PlayerCode,
                Username: playerInfo.Username,
                WalletId: playerInfo.DefaultWalletId,
                CurrencyCode: playerInfo.DefaultWalletCurrencyCode));
        }

        try
        {
            var formIds = await _playerFormRepository.ImportPlayerFormAddAsync(
                playerFormAdds,
                cancellationToken)
                .ToArrayAsync(
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unknown error occurred.({id})", command.Id);
        }

        var request = new ImportFormModelRequest
        {
            Id = form.Id
        };

        var subject = "game.playerbalance.import.form.processing";

        await _messageSender.PublishAsync(
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
