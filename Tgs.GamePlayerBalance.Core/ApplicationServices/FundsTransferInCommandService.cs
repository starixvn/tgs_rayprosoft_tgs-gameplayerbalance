using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameFundTransfer.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class FundsTransferInCommandService : ICommandService<FundsTransferInCommand>
{
    private readonly ILogger<FundsTransferInCommandService> _logger;
    private readonly IGameFundTransferClient _client;

    public FundsTransferInCommandService(
        ILogger<FundsTransferInCommandService> logger,
        IGameFundTransferClient client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(FundsTransferInCommand command, CancellationToken cancellationToken = default)
    {
        var reply = await _client
            .FundInZeroResultAsync(
                gameProviderId: command.GameProviderId,
                gameProviderCode: command.GameProviderCode,
                brandCode: command.BrandCode,
                playerCode: command.PlayerCode,
                walletId: command.WalletId,
                currencyCode: command.CurrencyCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("LoggerOn:{loggerOn} | Command:{command} | Result:{result}",
            DateTimeOffset.UtcNow.ToString("u"),
            JsonSerializer.Serialize(command),
            JsonSerializer.Serialize(reply));
    }
}
