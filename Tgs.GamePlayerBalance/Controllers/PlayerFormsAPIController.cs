using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/playerforms")]
[ApiController]
public class PlayerFormsAPIController : ControllerBase
{
    [HttpPost(Name = "PlayerFormAdd")]
    public ValueTask PlayerFormAddAsync(
        [FromServices] ICommandService<PlayerFormAddCommand> service,
        PlayerFormAddViewModel model)
    {
        var command = new PlayerFormAddCommand(
            GameProviderCode: model.GameProviderCode,
            PlayerCode: model.PlayerCode);
        return service
            .ExecuteAsync(
                command: command,
                cancellationToken: HttpContext.RequestAborted);
    }

    [HttpGet(Name = "PlayerFormQuery")]
    public IAsyncEnumerable<PlayerForm> BrandFormQueryAsync(
        [FromServices] IQueryService<PlayerFormQuery, PlayerForm> service,
        [FromQuery] PlayerFormQueryViewModel model)
        => service
        .QueryAsync(
            query: new PlayerFormQuery(
                model.PlayerCode,
                model.PageSize),
                cancellationToken: HttpContext.RequestAborted);
}
