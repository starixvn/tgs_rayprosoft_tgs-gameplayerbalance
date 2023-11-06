using MongoDB.Driver;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Extensions;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GamePlayerBalance.Repositories.Models;

namespace Tgs.GamePlayerBalance.Repositories;

internal class PlayerBalanceMongoRepository : IPlayerBalanceRepository
{
    private readonly IMongoDatabase _database;
    private readonly IPlayerBalanceRepository _repository;

    public PlayerBalanceMongoRepository(
        IPlayerBalanceRepository repository,
        IMongoDatabase database)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async ValueTask<PlayerBalance> GetAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var filter = Builders<PlayerTempBalanceModel>.Filter.Eq(x => x.GameProviderCode, gameProviderCode)
            & Builders<PlayerTempBalanceModel>.Filter.Eq(x => x.PlayerCode, playerCode);

        var options = new FindOptions<PlayerTempBalanceModel>
        {
            Skip = 0,
            Limit = 1
        };

        var asyncCursor = await collection.FindAsync(
            filter: filter,
            options: options,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        while (await asyncCursor
            .MoveNextAsync(
            cancellationToken: cancellationToken)
            .ConfigureAwait(false))
        {
            foreach (var current in asyncCursor.Current)
            {
                return new PlayerBalance(
                     current.Balance,
                     current.FreeBalance,
                     current.OutstandingBalance);
            }
        }

        return await _repository.GetAsync(
            playerCode: playerCode,
            gameProviderCode: gameProviderCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private IMongoCollection<PlayerTempBalanceModel> GetCollection()
        => _database.GetCollection<PlayerTempBalanceModel>(typeof(PlayerTempBalanceModel).GetTableName());
}
