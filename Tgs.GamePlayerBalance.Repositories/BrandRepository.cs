using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories;

internal class BrandRepository : IBrandRepository
{
    private readonly IGameCommandClient _client;

    public BrandRepository(
        IGameCommandClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<Guid> GetIdAsync(
        string code,
        CancellationToken cancellationToken = default)
        => await _client
        .BrandGetBrandIdAsync(
            code,
            cancellationToken)
        .ConfigureAwait(false)
        ?? throw new Exception($"The brand id does not exist.({code})");
}
