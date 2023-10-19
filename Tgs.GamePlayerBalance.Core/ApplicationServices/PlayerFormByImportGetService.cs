using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFormByImportGetService : IGetService<PlayerFormByImportQuery, DataPaginationResult<PlayerForm>>
{
    private readonly IPlayerFormRepository _repository;

    public PlayerFormByImportGetService(IPlayerFormRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask<DataPaginationResult<PlayerForm>> GetAsync(
        PlayerFormByImportQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = _repository.ImportQueryAsync(
            expansionFormId: query.ExpansionFormId,
            state: query.State,
            playerCode: query.PlayerCode,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken)
            .ToEnumerable()
            ?? Enumerable.Empty<PlayerForm>();

        var count = await _repository.ImportQueryCountAsync(
            expansionFormId: query.ExpansionFormId,
            state: query.State,
            playerCode: query.PlayerCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new DataPaginationResult<PlayerForm>(
            Items: items,
            Page: query.Page,
            PageSize: query.PageSize,
            TotalPage: (count + query.PageSize - 1) / query.PageSize,
            TotalCount: count);
    }
}
