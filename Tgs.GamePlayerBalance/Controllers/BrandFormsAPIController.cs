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
        BrandFormAddViewModel source)
    {
        var command = new BrandFormAddCommand(
            GameProviderCode: source.GameProviderCode,
            BrandCode: source.BrandCode);
        return service
            .ExecuteAsync(
                command: command,
                cancellationToken: HttpContext.RequestAborted);
    }

    [HttpGet(Name = "BrandFormQuery")]
    public async IAsyncEnumerable<BrandFormViewModel> BrandFormQueryAsync(
        [FromServices] IQueryService<BrandFormQuery, BrandForm> service,
        [FromQuery] BrandFormQueryViewModel source)
    {
        var forms = await service
          .QueryAsync(
              query: new BrandFormQuery(
                   BrandCode: source.BrandCode,
                   State: source.State,
                  PageSize: source.PageSize),
              cancellationToken: HttpContext.RequestAborted)
          .ToArrayAsync(
            cancellationToken: HttpContext.RequestAborted)
          .ConfigureAwait(false)
          ?? Enumerable.Empty<BrandForm>();

        if (!forms.Any())
        {
            yield break;
        }

        foreach (var form in forms)
        {
            yield return new BrandFormViewModel(
                Id: form.Id.ToString(),
                GameProviderId: form.GameProviderId,
                GameProviderCode: form.GameProviderCode,
                BrandId: form.BrandId,
                BrandCode: form.BrandCode,
                State: form.State,
                Status: form.Status,
                CreatedOn: form.CreatedOn,
                UpdateOn: form.UpdateOn,
                StartOn: form.StartOn,
                EndOn: form.EndOn,
                ExpireOn: form.ExpireOn);
        }
    }

    [HttpGet("state")]
    public IAsyncEnumerable<SelectViewModel> StateAsync()
    => Enum
        .GetValues(typeof(BrandFormState))
        .Cast<BrandFormState>()
        .Select(x => new SelectViewModel(x.ToString(), x.ToString()))
        .ToAsyncEnumerable();
}
