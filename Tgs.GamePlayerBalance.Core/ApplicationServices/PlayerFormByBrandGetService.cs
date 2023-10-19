using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFormByBrandGetService : IGetService<PlayerFormByBrandQuery, DataPaginationResult<PlayerForm>>
{
    private readonly IPlayerFormRepository _repository;

    public PlayerFormByBrandGetService(IPlayerFormRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask<DataPaginationResult<PlayerForm>> GetAsync(
        PlayerFormByBrandQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = _repository.BrandQueryAsync(
            expansionFormId: query.ExpansionFormId,
            state: query.State,
            playerCode: query.PlayerCode,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken)
            .ToEnumerable()
            ?? Enumerable.Empty<PlayerForm>();

        var count = await _repository.BrandQueryCountAsync(
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
