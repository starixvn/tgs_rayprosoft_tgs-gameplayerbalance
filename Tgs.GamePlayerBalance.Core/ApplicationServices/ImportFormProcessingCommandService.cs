using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class ImportFormProcessingCommandService : ICommandService<ImportFormProcessingCommand>
{
    private readonly IImportFormRepository _importFormRepository;
    private readonly ILogger<ImportFormProcessingCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IPlayerFormRepository _playerFormRepository;

    public ImportFormProcessingCommandService(
        ILogger<ImportFormProcessingCommandService> logger,
        IImportFormRepository importFormRepository,
        IPlayerFormRepository playerFormRepository,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _importFormRepository = importFormRepository ?? throw new ArgumentNullException(nameof(importFormRepository));
        _playerFormRepository = playerFormRepository ?? throw new ArgumentNullException(nameof(playerFormRepository));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask ExecuteAsync(
        ImportFormProcessingCommand command,
        CancellationToken cancellationToken = default)
    {
        var form = await _importFormRepository
            .GetAsync(
                id: command.Id,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (form == null)
        {
            _logger.LogError("This brand form does not exist.({id})", command.Id);
            throw new Exception($"This brand form does not exist.({command.Id})");
        }

        if (form.State != ImportFormState.Processing)
        {
            _logger.LogError("The brand form status is not processing.({id})", command.Id);
            throw new Exception($"The brand form status is not processing.({command.Id})");
        }

        var pageSize = 50;
        try
        {
            var filterId = await _playerFormRepository
                .ImportGetFirstIdAsync(
                    expansionFormId: form.Id,
                    state: PlayerFormState.Queued,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? 0;

            if (filterId == 0)
            {
                _logger.LogWarning("There is no queue list under this brand list.({id})", command.Id);
                return;
            }

            filterId--;

            while (filterId > 0)
            {
                var formIds = await _playerFormRepository
                .ImportQueryIdAsync(
                    expansionFormId: form.Id,
                    state: PlayerFormState.Queued,
                    id: filterId,
                    pageSize: pageSize,
                    cancellationToken: cancellationToken)
                .ToArrayAsync(
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? Enumerable.Empty<long>();

                if (!formIds.Any())
                {
                    break;
                }

                foreach (var formId in formIds)
                {
                    filterId = formId;
                    try
                    {
                        var request = new PlayerFormModelRequest
                        {
                            Id = formId
                        };

                        await _messageSender
                            .PublishAsync(
                                subject: "game.playerbalance.player.form.queued",
                                data: request.ToByteArray(),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        _logger.LogInformation("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An exception occurred when sending the player application form.({id})", formId);
                    }
                }
            }

            await _importFormRepository
                 .UpdateCompleteAsync(
                     id: command.Id,
                     cancellationToken: cancellationToken)
                 .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unknown error occurred.({id})", command.Id);

            await _importFormRepository
                 .UpdateFailedAsync(
                     id: command.Id,
                     status: ex.Message,
                     cancellationToken: cancellationToken)
                 .ConfigureAwait(false);
        }
    }
}
