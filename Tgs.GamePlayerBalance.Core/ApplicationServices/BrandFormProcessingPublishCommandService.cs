using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormProcessingPublishCommandService : ICommandService<BrandFormProcessingPublishCommand>
{
    private readonly ILogger<BrandFormProcessingPublishCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IPlayerFormRepository _playerFormRepository;

    public BrandFormProcessingPublishCommandService(
        ILogger<BrandFormProcessingPublishCommandService> logger,
        IPlayerFormRepository playerFormRepository,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _playerFormRepository = playerFormRepository ?? throw new ArgumentNullException(nameof(playerFormRepository));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask ExecuteAsync(
        BrandFormProcessingPublishCommand command,
        CancellationToken cancellationToken = default)
    {
        var pageSize = 50;
        var id = command.FilterId;

        while (id > 0)
        {
            var formIds = await _playerFormRepository.BrandQueryIdAsync(
                expansionFormId: command.ExpansionFormId,
                state: PlayerFormState.Queued,
                id: id,
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
                id = formId;
                try
                {
                    var request = new PlayerFormModelRequest
                    {
                        Id = formId
                    };

                    await _messageSender.PublishAsync(
                        subject: "game.playerbalance.player.form.queued",
                        data: request.ToByteArray(),
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    _logger.LogDebug("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occurred when sending the player application form.({id})", formId);
                }
            }
        }
    }
}
