using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.HandleWorkServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayersController : ControllerBase
{
    [HttpPost(Name = "Player")]
    public ValueTask PlayerAsync(
        [FromServices] ICommandService<PlayerHadleWorkCommand> service,
        PlayersViewModel model)
    {
        var command = new PlayerHadleWorkCommand(
            GameProviderCode: model.GameProviderCode,
            PlayerCode: model.PlayerCode);
        return service
            .ExecuteAsync(
                command: command,
                cancellationToken: HttpContext.RequestAborted);
    }
}
