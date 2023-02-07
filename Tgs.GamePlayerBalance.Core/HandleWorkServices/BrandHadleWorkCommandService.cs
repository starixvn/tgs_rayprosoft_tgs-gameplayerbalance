using Microsoft.Extensions.Logging;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.HandleWorkServices;

internal class BrandHadleWorkCommandService : ICommandService<BrandHadleWorkCommand>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IGameCommandClient _client;
    private readonly ILogger<AwcAelottoHadleWorkCommandService> _logger;

    public BrandHadleWorkCommandService(
        ILogger<AwcAelottoHadleWorkCommandService> logger,
        IGameCommandClient client,
        IBrandRepository brandRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
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
        }
    }
}
