using System.Runtime.CompilerServices;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerFormQueryService : IQueryService<PlayerFormQuery, PlayerForm>
{
    private readonly IPlayerFormRepository _repository;

    public PlayerFormQueryService(
        IPlayerFormRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async IAsyncEnumerable<PlayerForm> QueryAsync(
        PlayerFormQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var forms = await _repository.QueryAsync(
            playerCode: query.PlayerCode,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken)
            .ToArrayAsync(
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!forms.Any())
        {
            yield break;
        }

        foreach (var form in forms)
        {
            yield return form;
        }
    }
}
