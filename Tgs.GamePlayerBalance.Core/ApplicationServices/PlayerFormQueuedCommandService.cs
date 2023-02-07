using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFormQueuedCommandService : ICommandService<PlayerFormQueuedCommand>
{
    private readonly IGameProvidersClient _client;
    private readonly ILogger<PlayerFormQueuedCommandService> _logger;
    private readonly IPlayerFormRepository _repository;
    private readonly IGetService<FundsTransferInQuery, FundsTransferInResult> _service;

    public PlayerFormQueuedCommandService(
        ILogger<PlayerFormQueuedCommandService> logger,
        IGameProvidersClient client,
        IPlayerFormRepository repository,
        IGetService<FundsTransferInQuery, FundsTransferInResult> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PlayerFormQueuedCommand command, CancellationToken cancellationToken = default)
    {
        var form = await _repository
            .GetAsync(
                id: command.Id,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (form == null)
        {
            _logger.LogError("This player form does not exist.({id})", command.Id);
            throw new Exception($"This player form does not exist.({command.Id})");
        }

        if (form.State != PlayerFormState.Queued)
        {
            _logger.LogError("The player form status is not queued.({id})", command.Id);
            throw new Exception($"The player form status is not queued.({command.Id})");
        }

        await _repository
            .UpdateProcessingAsync(
                id: command.Id,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var balance = await _client
            .PlayerBalanceAsync(
                gameProviderCode: form.GameProviderCode,
                playerCode: form.PlayerCode)
            .ConfigureAwait(false);
        try
        {
            if (balance > 0)
            {
                var query = new FundsTransferInQuery(
                    form.GameProviderId,
                    form.GameProviderCode,
                    BrandCode: form.BrandCode,
                    PlayerCode: form.PlayerCode,
                    WalletId: form.WalletId,
                    CurrencyCode: form.CurrencyCode);

                var result = await _service
                      .GetAsync(
                          query: query,
                          cancellationToken: cancellationToken)
                      .ConfigureAwait(false);

                await _repository
                    .UpdateCompleteAsync(
                        id: command.Id,
                        resultStateCode: result.StateCode,
                        resultMessage: result.Message,
                        resultFundTransactionId: result.FundTransactionId,
                        resultTransactionId: result.TransactionId,
                        resultExternalTransactionId: result.ExternalTransactionId,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await _repository
                    .UpdateCompleteAsync(
                        id: command.Id,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unknown error occurred.({id})", command.Id);

            await _repository
                 .UpdateFailedAsync(
                     id: command.Id,
                     cancellationToken: cancellationToken)
                 .ConfigureAwait(false);
        }
    }
}
