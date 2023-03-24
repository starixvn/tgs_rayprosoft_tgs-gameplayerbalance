using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormProcessingCommandService : ICommandService<BrandFormProcessingCommand>
{
    private readonly IBrandFormRepository _brandFormRepository;
    private readonly ILogger<BrandFormProcessingCommandService> _logger;
    private readonly IPlayerFormRepository _playerFormRepository;
    private readonly ICommandService<BrandFormProcessingPublishCommand> _service;

    public BrandFormProcessingCommandService(
        ILogger<BrandFormProcessingCommandService> logger,
        IBrandFormRepository brandFormRepository,
        IPlayerFormRepository playerFormRepository,
        ICommandService<BrandFormProcessingPublishCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _brandFormRepository = brandFormRepository ?? throw new ArgumentNullException(nameof(brandFormRepository));
        _playerFormRepository = playerFormRepository ?? throw new ArgumentNullException(nameof(playerFormRepository));
        _service = service ?? throw new ArgumentNullException(nameof(service));
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

        try
        {
            var filterId = await _playerFormRepository
                .BrandGetFirstIdAsync(
                    expansionFormId: form.Id,
                    state: PlayerFormState.Queued,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? 0;

            if (filterId > 0)
            {
                filterId--;
                await _service
                    .ExecuteAsync(
                        command: new BrandFormProcessingPublishCommand(
                            ExpansionFormId: form.Id,
                            FilterId: filterId),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
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
                     status: ex.Message,
                     cancellationToken: cancellationToken)
                 .ConfigureAwait(false);
        }
    }
}
