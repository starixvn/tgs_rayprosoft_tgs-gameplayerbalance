using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class ImportFormProcessingCommandService : ICommandService<ImportFormProcessingCommand>
{
    private readonly IImportFormRepository _importFormRepository;
    private readonly ILogger<ImportFormProcessingCommandService> _logger;
    private readonly IPlayerFormRepository _playerFormRepository;
    private readonly ICommandService<ImportFormProcessingPublishCommand> _service;

    public ImportFormProcessingCommandService(
        ILogger<ImportFormProcessingCommandService> logger,
        IImportFormRepository importFormRepository,
        IPlayerFormRepository playerFormRepository,
        ICommandService<ImportFormProcessingPublishCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _importFormRepository = importFormRepository ?? throw new ArgumentNullException(nameof(importFormRepository));
        _playerFormRepository = playerFormRepository ?? throw new ArgumentNullException(nameof(playerFormRepository));
        _service = service ?? throw new ArgumentNullException(nameof(service));
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

        try
        {
            var filterId = await _playerFormRepository
                .ImportGetFirstIdAsync(
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
                        command: new ImportFormProcessingPublishCommand(
                            ExpansionFormId: form.Id,
                            FilterId: filterId),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
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
