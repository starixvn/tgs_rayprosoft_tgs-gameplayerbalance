using Microsoft.AspNetCore.Mvc;
using Tgs.GameCommand.Client;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/gameproviders")]
[ApiController]
public class GameProvidersAPIController : ControllerBase
{
    private readonly IGameCommandClient _client;

    public GameProvidersAPIController(IGameCommandClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    [HttpGet]
    public async IAsyncEnumerable<string> QueryAsync()
    {
        var gameProviderIds = await _client
            .GameProviderGetGameProviderIdsAsync(
                HttpContext.RequestAborted)
            .ToArrayAsync(
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (!gameProviderIds.Any())
        {
            yield break;
        }

        foreach (var gameProviderId in gameProviderIds)
        {
            yield return gameProviderId.GameProviderCode;
        }
    }
}
