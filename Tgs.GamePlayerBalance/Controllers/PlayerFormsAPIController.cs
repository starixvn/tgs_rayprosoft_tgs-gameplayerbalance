using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/playerforms")]
[ApiController]
public class PlayerFormsAPIController : ControllerBase
{
    [HttpGet(Name = "PlayerFormQuery")]
    public async IAsyncEnumerable<PlayerFormViewModel> BrandFormQueryAsync(
        [FromServices] IQueryService<PlayerFormQuery, PlayerForm> service,
        [FromQuery] PlayerFormQueryViewModel source)
    {
        var forms = await service
          .QueryAsync(
              query: new PlayerFormQuery(
                  source.PlayerCode,
                  source.PageSize),
              cancellationToken: HttpContext.RequestAborted)
          .ToArrayAsync(
            cancellationToken: HttpContext.RequestAborted)
          .ConfigureAwait(false)
          ?? Enumerable.Empty<PlayerForm>();

        if (!forms.Any())
        {
            yield break;
        }

        foreach (var form in forms)
        {
            yield return new PlayerFormViewModel(
                Id: form.Id.ToString(),
                ExpansionFormId: form.ExpansionFormId.ToString(),
                GameProviderId: form.GameProviderId,
                GameProviderCode: form.GameProviderCode,
                BrandId: form.BrandId,
                BrandCode: form.BrandCode,
                PlayerCode: form.PlayerCode,
                Username: form.Username,
                WalletId: form.WalletId,
                CurrencyCode: form.CurrencyCode,
                State: form.State,
                Status: form.Status,
                CreatedOn: form.CreatedOn,
                UpdateOn: form.UpdateOn,
                StartOn: form.StartOn,
                EndOn: form.EndOn,
                ExpireOn: form.ExpireOn,
                Result: form.Result);
        }
    }

    [HttpPost(Name = "PlayerFormAdd")]
    public ValueTask PlayerFormAddAsync(
        [FromServices] ICommandService<PlayerFormAddCommand> service,
        PlayerFormAddViewModel source)
    {
        var command = new PlayerFormAddCommand(
            GameProviderCode: source.GameProviderCode,
            PlayerCode: source.PlayerCode);
        return service
            .ExecuteAsync(
                command: command,
                cancellationToken: HttpContext.RequestAborted);
    }

    [HttpGet("brand/{expansionFormId}")]
    public async ValueTask<DataPaginationViewModel<PlayerFormViewModel>> QueryAsync(
        [FromServices] IGetService<PlayerFormByBrandQuery, DataPaginationResult<PlayerForm>> service,
        long expansionFormId,
        [FromQuery] PlayerFormState? state,
        [FromQuery] string? playerCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 1000)
    {
        var result = await service
            .GetAsync(
                query: new PlayerFormByBrandQuery(
                    ExpansionFormId: expansionFormId,
                    State: state,
                    PlayerCode: playerCode,
                    Page: page,
                    PageSize: pageSize),
                cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var viewModels = new List<PlayerFormViewModel>();

        if (result.Items.Any())
        {
            foreach (var item in result.Items)
            {
                viewModels.Add(new PlayerFormViewModel(
                    Id: item.Id.ToString(),
                    ExpansionFormId: item.ExpansionFormId.ToString(),
                    GameProviderId: item.GameProviderId,
                    GameProviderCode: item.GameProviderCode,
                    BrandId: item.BrandId,
                    BrandCode: item.BrandCode,
                    PlayerCode: item.PlayerCode,
                    Username: item.Username,
                    WalletId: item.WalletId,
                    CurrencyCode: item.CurrencyCode,
                    State: item.State,
                    Status: item.Status,
                    CreatedOn: item.CreatedOn,
                    UpdateOn: item.UpdateOn,
                    StartOn: item.StartOn,
                    EndOn: item.EndOn,
                    ExpireOn: item.ExpireOn,
                    Result: item.Result));
            }
        }

        return new DataPaginationViewModel<PlayerFormViewModel>(
            Items: viewModels,
            Page: result.Page,
            PageSize: result.PageSize,
            TotalPage: result.TotalPage,
            TotalCount: result.TotalCount);
    }

    [HttpGet("import/{expansionFormId}")]
    public async ValueTask<DataPaginationViewModel<PlayerFormViewModel>> QueryAsync(
        [FromServices] IGetService<PlayerFormByImportQuery, DataPaginationResult<PlayerForm>> service,
        long expansionFormId,
        [FromQuery] PlayerFormState? state,
        [FromQuery] string? playerCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 1000)
    {
        var result = await service
            .GetAsync(
                query: new PlayerFormByImportQuery(
                    ExpansionFormId: expansionFormId,
                    State: state,
                    PlayerCode: playerCode,
                    Page: page,
                    PageSize: pageSize),
                cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var viewModels = new List<PlayerFormViewModel>();

        if (result.Items.Any())
        {
            foreach (var item in result.Items)
            {
                viewModels.Add(new PlayerFormViewModel(
                    item.Id.ToString(),
                    item.ExpansionFormId.ToString(),
                    item.GameProviderId,
                    item.GameProviderCode,
                    item.BrandId,
                    item.BrandCode,
                    item.PlayerCode,
                    item.Username,
                    item.WalletId,
                    item.CurrencyCode,
                    item.State,
                    item.Status,
                    item.CreatedOn,
                    item.UpdateOn,
                    item.StartOn,
                    item.EndOn,
                    item.ExpireOn,
                    item.Result));
            }
        }

        return new DataPaginationViewModel<PlayerFormViewModel>(
            Items: viewModels,
            Page: result.Page,
            PageSize: result.PageSize,
            TotalPage: result.TotalPage,
            TotalCount: result.TotalCount);
    }
}
