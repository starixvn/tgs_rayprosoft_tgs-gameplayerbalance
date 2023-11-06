using MongoDB.Driver;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Extensions;
using Tgs.GamePlayerBalance.Repositories.Models;

namespace Tgs.GamePlayerBalance.Repositories;

internal class PlayerTempBalanceRepository : IPlayerTempBalanceRepository
{
    private readonly IMongoDatabase _database;

    public PlayerTempBalanceRepository(
        IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async ValueTask AddAsync(string gameProviderCode, string playerCode, decimal balance, decimal freeBalance, decimal outstandingBalance, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var document = new PlayerTempBalanceModel
        {
            GameProviderCode = gameProviderCode,
            PlayerCode = playerCode,
            Balance = balance,
            FreeBalance = freeBalance,
            OutstandingBalance = outstandingBalance,
            CreatedOn = DateTime.UtcNow,
            ExpireOn = DateTime.UtcNow.AddSeconds(30)
        };

        await collection.InsertOneAsync(
            document: document,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask DeleteAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var filter = Builders<PlayerTempBalanceModel>.Filter.Eq(x => x.GameProviderCode, gameProviderCode)
            & Builders<PlayerTempBalanceModel>.Filter.Eq(x => x.PlayerCode, playerCode);

        _ = await collection.DeleteOneAsync(
            filter: filter,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<bool> ExistAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default)
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
                return true;
            }
        }
        return false;
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        await Index_UniqueAsync(
            collection: collection,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await Index_ExpireOnAsync(
            collection: collection,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await Index_CreatedOnAsync(
            collection: collection,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private static Task Index_CreatedOnAsync(
        IMongoCollection<PlayerTempBalanceModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerTempBalanceModel>.IndexKeys
            .Descending(m => m.CreatedOn);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = "idx_created_on"
        };
        var createIndexModel = new CreateIndexModel<PlayerTempBalanceModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task Index_ExpireOnAsync(
            IMongoCollection<PlayerTempBalanceModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerTempBalanceModel>.IndexKeys
            .Descending(m => m.ExpireOn);

        var createIndexOptions = new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.FromSeconds(1),
            Name = "idx_expire_on"
        };
        var createIndexModel = new CreateIndexModel<PlayerTempBalanceModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task Index_UniqueAsync(
        IMongoCollection<PlayerTempBalanceModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerTempBalanceModel>.IndexKeys
            .Ascending(m => m.GameProviderCode)
            .Ascending(m => m.PlayerCode);

        var createIndexOptions = new CreateIndexOptions
        {
            Unique = true,
            Name = $"uni_{collection.CollectionNamespace.CollectionName}"
        };
        var createIndexModel = new CreateIndexModel<PlayerTempBalanceModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private IMongoCollection<PlayerTempBalanceModel> GetCollection()
        => _database.GetCollection<PlayerTempBalanceModel>(typeof(PlayerTempBalanceModel).GetTableName());
}
