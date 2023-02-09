using System.Runtime.CompilerServices;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormQueryService : IQueryService<BrandFormQuery, BrandForm>
{
    private readonly IGameCommandClient _client;
    private readonly IBrandFormRepository _repository;

    public BrandFormQueryService(
        IGameCommandClient client,
        IBrandFormRepository repository)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async IAsyncEnumerable<BrandForm> QueryAsync(
        BrandFormQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var brandId = await _client
            .BrandGetBrandIdAsync(
                brandcode: query.BrandCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (!brandId.HasValue)
        {
            yield break;
        }

        var brandForms = await _repository
            .QueryAsync(
                brandId: brandId.Value,
                state: query.State,
                pageSize: query.pageSize,
                cancellationToken: cancellationToken)
            .ToArrayAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!brandForms.Any())
        {
            yield break;
        }

        foreach (var form in brandForms)
        {
            yield return form;
        }
    }
}
