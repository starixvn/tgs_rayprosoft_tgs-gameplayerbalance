using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/brandforms")]
[ApiController]
public class BrandFormsAPIController : ControllerBase
{
    [HttpPost(Name = "BrandFormAdd")]
    public ValueTask BrandFormAddAsync(
        [FromServices] ICommandService<BrandFormAddCommand> service,
        BrandFormAddViewModel model)
    {
        var command = new BrandFormAddCommand(
            GameProviderCode: model.GameProviderCode,
            BrandCode: model.BrandCode);
        return service
            .ExecuteAsync(
                command: command,
                cancellationToken: HttpContext.RequestAborted);
    }
}
