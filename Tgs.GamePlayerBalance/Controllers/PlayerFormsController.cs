using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerFormsController : ControllerBase
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
}
