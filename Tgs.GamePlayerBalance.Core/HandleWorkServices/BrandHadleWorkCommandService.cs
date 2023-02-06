using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.HandleWorkServices;

internal class BrandHadleWorkCommandService : ICommandService<BrandHadleWorkCommand>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IGameCommandClient _client;
    private readonly ILogger<AwcAelottoHadleWorkCommandService> _logger;
    private readonly ICommandService<PlayerCommand> _service;

    public BrandHadleWorkCommandService(
        ILogger<AwcAelottoHadleWorkCommandService> logger,
        IGameCommandClient client,
        IBrandRepository brandRepository,
        ICommandService<PlayerCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(BrandHadleWorkCommand command, CancellationToken cancellationToken = default)
    {
        var brandId = await _brandRepository
            .GetIdAsync(
                code: command.BrandCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var pageIndex = 1;
        var pageSize = 1000;
        while (true)
        {
            var players = await _client
                .PlayerQueryAsync(
                    brandId: brandId,
                    pageIndex: pageIndex,
                    pageSize: pageSize,
                    cancellationToken: cancellationToken)
                .ToArrayAsync()
                .ConfigureAwait(false)
                ?? Enumerable.Empty<PlayerInfo>();

            if (!players.Any())
            {
                break;
            }

            pageIndex++;

            foreach (var player in players)
            {
                try
                {
                    var playerCommand = new PlayerCommand(
                        command.GameProviderCode,
                        player.BrandCode,
                        player.PlayerCode,
                        player.DefaultWalletId,
                        player.DefaultWalletCurrencyCode);

                    await _service
                         .ExecuteAsync(
                             command: playerCommand,
                             cancellationToken: cancellationToken)
                         .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "The player's balance withdrawal process failed.({playerCode})", player.PlayerCode);
                }
            }
        }
    }
}
