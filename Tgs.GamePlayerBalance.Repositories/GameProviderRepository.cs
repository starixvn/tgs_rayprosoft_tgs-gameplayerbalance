using System.Text.Json;
using System.Text.Json.Serialization;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GameProviders.Repositories;

internal class GameProviderRepository : IGameProviderRepository
{
    private static readonly JsonSerializerOptions _SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly IGameCommandClient _client;

    public GameProviderRepository(IGameCommandClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<Guid> GetIdAsync(
        string code,
        CancellationToken cancellationToken = default)
        => await _client
        .GameProviderGetGameProviderIdAsync(
            gameProviderCode: code,
            cancellationToken: cancellationToken)
        .ConfigureAwait(false)
        ?? throw new Exception($"Game Provider does not exist.({code})");
}
