using System.Runtime.CompilerServices;
using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class ImportFormQuerySerivce : IQueryService<ImportFormQuery, ImportForm>
{
    private readonly IGameProviderRepository _gameProviderRepository;
    private readonly IImportFormRepository _importFormRepository;

    public ImportFormQuerySerivce(
        IGameProviderRepository gameProviderRepository,
        IImportFormRepository importFormRepository)
    {
        _gameProviderRepository = gameProviderRepository ?? throw new ArgumentNullException(nameof(gameProviderRepository));
        _importFormRepository = importFormRepository ?? throw new ArgumentNullException(nameof(importFormRepository));
    }

    public async IAsyncEnumerable<ImportForm> QueryAsync(
        ImportFormQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _gameProviderRepository
            .GetIdAsync(
                code: query.GameProviderCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var forms = await _importFormRepository
            .QueryAsync(
                gameProviderId: gameProviderId,
                state: query.State,
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
