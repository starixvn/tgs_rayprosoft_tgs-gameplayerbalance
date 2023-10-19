using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class ImportFormAddCommandService : ICommandService<ImportFormAddCommand>
{
    private readonly IGameProviderRepository _gameProviderRepository;
    private readonly IImportFormRepository _importFormRepository;
    private readonly ILogger<ImportFormAddCommandService> _logger;
    private readonly IMessageSender _messageSender;

    public ImportFormAddCommandService(
        ILogger<ImportFormAddCommandService> logger,
        IImportFormRepository importFormRepository,
        IGameProviderRepository gameProviderRepository,
        IMessageSender messageSender
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _importFormRepository = importFormRepository ?? throw new ArgumentNullException(nameof(importFormRepository));
        _gameProviderRepository = gameProviderRepository ?? throw new ArgumentNullException(nameof(gameProviderRepository));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask ExecuteAsync(ImportFormAddCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _gameProviderRepository
           .GetIdAsync(
               code: command.GameProviderCode,
               cancellationToken: cancellationToken)
           .ConfigureAwait(false);

        var id = await _importFormRepository
            .AddAsync(
                gameProviderId: gameProviderId,
                gameProviderCode: command.GameProviderCode,
                players: command.Players,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var request = new ImportFormModelRequest
        {
            Id = id
        };

        var subject = "game.playerbalance.import.form.queued";

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
