using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/excelforms")]
[ApiController]
public class ExcelFormsAPIController : ControllerBase
{
    [HttpGet(Name = "ImportFormQuery")]
    public async IAsyncEnumerable<ImportFormViewModel> BrandFormQueryAsync(
        [FromServices] IQueryService<ImportFormQuery, ImportForm> service,
        [FromQuery] ImportFormQueryViewModel model)
    {
        var forms = await service
          .QueryAsync(
              query: new ImportFormQuery(
                   GameProviderCode: model.GameProviderCode,
                   State: model.State,
                  PageSize: model.PageSize),
              cancellationToken: HttpContext.RequestAborted)
          .ToArrayAsync(
            cancellationToken: HttpContext.RequestAborted)
          .ConfigureAwait(false)
          ?? Enumerable.Empty<ImportForm>();

        if (!forms.Any())
        {
            yield break;
        }

        foreach (var form in forms)
        {
            yield return new ImportFormViewModel(
                Id: form.Id.ToString(),
                GameProviderId: form.GameProviderId,
                GameProviderCode: form.GameProviderCode,
                State: form.State,
                Status: form.Status,
                Players: form.Players,
                CreatedOn: form.CreatedOn,
                UpdateOn: form.UpdateOn,
                StartOn: form.StartOn,
                EndOn: form.EndOn,
                ExpireOn: form.ExpireOn);
        }
    }
}
