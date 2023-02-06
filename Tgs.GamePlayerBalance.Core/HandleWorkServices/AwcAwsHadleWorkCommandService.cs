using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.HandleWorkServices;

internal class AwcAwsHadleWorkCommandService : ICommandService<AwcAwsHadleWorkCommand>
{
    private readonly IGameProvidersBalancesClient _client;
    private readonly ILogger<AwcAelottoHadleWorkCommandService> _logger;
    private readonly ICommandService<PlayerBakanceCommand> _service;

    public AwcAwsHadleWorkCommandService(
        ILogger<AwcAelottoHadleWorkCommandService> logger,
        IGameProvidersBalancesClient client,
        ICommandService<PlayerBakanceCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(AwcAwsHadleWorkCommand command, CancellationToken cancellationToken = default)
    {
        var balances = await _client
             .AwcAwsGetBalances(
                brandCode: command.BrandCode,
                cancellationToken: cancellationToken)
             .ToArrayAsync()
             .ConfigureAwait(false)
             ?? Enumerable.Empty<BalanceResponse>();

        if (!balances.Any())
        {
            _logger.LogInformation("Players with no remaining balance from game providers under this brand.");
            return;
        }

        foreach (var balance in balances)
        {
            var playerBakance = new PlayerBakanceCommand(
                GameProviderCode: command.GameProviderCode,
                PlayerCode: balance.PlayerCode);
            try
            {
                await _service
                    .ExecuteAsync(
                        playerBakance)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The player's balance withdrawal process failed.({playerCode})", balance.PlayerCode);
            }
        }
    }
}
