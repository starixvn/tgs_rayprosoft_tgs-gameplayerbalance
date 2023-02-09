using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.Core.Models;
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

    [HttpGet(Name = "BrandFormQuery")]
    public IAsyncEnumerable<BrandForm> BrandFormQueryAsync(
        [FromServices] IQueryService<BrandFormQuery, BrandForm> service,
        [FromQuery] BrandFormQueryViewModel model)
        => service
        .QueryAsync(
            query: new BrandFormQuery(
                model.BrandCode,
                model.State,
                model.pageSize),
                cancellationToken: HttpContext.RequestAborted);

    [HttpGet("state")]
    public IAsyncEnumerable<SelectViewModel> StateAsync()
    => Enum
        .GetValues(typeof(BrandFormState))
        .Cast<BrandFormState>()
        .Select(x => new SelectViewModel(x.ToString(), x.ToString()))
        .ToAsyncEnumerable();
}
