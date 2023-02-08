using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormProcessingCommandService : ICommandService<BrandFormProcessingCommand>
{
    private readonly IBrandFormRepository _brandFormRepository;
    private readonly ILogger<BrandFormProcessingCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IPlayerFormRepository _playerFormRepository;

    public BrandFormProcessingCommandService(
        ILogger<BrandFormProcessingCommandService> logger,
        IBrandFormRepository brandFormRepository,
        IPlayerFormRepository playerFormRepository,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _brandFormRepository = brandFormRepository ?? throw new ArgumentNullException(nameof(brandFormRepository));
        _playerFormRepository = playerFormRepository ?? throw new ArgumentNullException(nameof(playerFormRepository));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask ExecuteAsync(
        BrandFormProcessingCommand command,
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

        if (form.State != BrandFormState.Processing)
        {
            _logger.LogError("The brand form status is not processing.({id})", command.Id);
            throw new Exception($"The brand form status is not processing.({command.Id})");
        }

        var pageSize = 1000;
        try
        {
            var filterId = await _playerFormRepository
                .BrandGetFirstIdAsync(
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
                .BrandQueryIdAsync(
                    expansionFormId: form.Id,
                    state: PlayerFormState.Queued,
                    id: filterId,
                    pageSize: pageSize,
                    cancellationToken: cancellationToken)
                .ToArrayAsync()
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

            await _brandFormRepository
                 .UpdateCompleteAsync(
                     id: command.Id,
                     cancellationToken: cancellationToken)
                 .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unknown error occurred.({id})", command.Id);

            await _brandFormRepository
                 .UpdateFailedAsync(
                     id: command.Id,
                     cancellationToken: cancellationToken)
                 .ConfigureAwait(false);
        }
    }
}
