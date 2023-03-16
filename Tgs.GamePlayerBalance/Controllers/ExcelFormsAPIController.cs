using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core;
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
    public IAsyncEnumerable<ImportForm> BrandFormQueryAsync(
        [FromServices] IQueryService<ImportFormQuery, ImportForm> service,
        [FromQuery] ImportFormQueryViewModel model)
        => service
        .QueryAsync(
            query: new ImportFormQuery(
                model.GameProviderCode,
                model.State,
                model.PageSize),
                cancellationToken: HttpContext.RequestAborted);

    [HttpGet("state")]
    public IAsyncEnumerable<SelectViewModel> StateAsync()
    => Enum
        .GetValues(typeof(ImportFormState))
        .Cast<BrandFormState>()
        .Select(x => new SelectViewModel(x.ToString(), x.ToString()))
        .ToAsyncEnumerable();
}
