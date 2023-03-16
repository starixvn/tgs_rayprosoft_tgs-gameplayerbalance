using System.Runtime.CompilerServices;
using MongoDB.Driver;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GamePlayerBalance.Repositories.Models;

namespace Tgs.GamePlayerBalance.Repositories;

internal class BrandFormRepository : IBrandFormRepository
{
    private readonly IMongoDatabase _database;
    private readonly ISnowflake _snowflake;

    public BrandFormRepository(
        IMongoDatabase database,
        ISnowflake snowflake)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _snowflake = snowflake ?? throw new ArgumentNullException(nameof(snowflake));
    }

    public async ValueTask<long> AddAsync(
        Guid gameProviderId,
        string gameProviderCode,
        Guid brandId,
        string brandCode,
        CancellationToken cancellationToken = default)
    {
        var id = _snowflake.NextId();

        var collection = GetCollection();

        var document = new BrandFormModel
        {
            Id = id,
            GameProviderId = gameProviderId,
            GameProviderCode = gameProviderCode,
            BrandId = brandId,
            BrandCode = brandCode,
            State = BrandFormState.Queued,
            Status = $"{BrandFormState.Queued}...",
            CreatedOn = DateTime.UtcNow,
            UpdateOn = DateTime.UtcNow,
            ExpireOn = null
        };

        await collection
             .InsertOneAsync(
                 document: document,
                 cancellationToken: cancellationToken)
             .ConfigureAwait(false);

        return id;
    }

    public async ValueTask<BrandForm?> GetAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var builder = Builders<BrandFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var options = new FindOptions<BrandFormModel>
        {
            Skip = 0,
            Limit = 1
        };
        var datas = await collection
                    .FindAsync(
                        filter: filter,
                        options: options,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

        var data = await datas
                        .FirstOrDefaultAsync(
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

        return data == null
            ? null
            : new BrandForm(
                Id: data.Id,
                GameProviderId: data.GameProviderId,
                GameProviderCode: data.GameProviderCode,
                BrandId: data.BrandId,
                BrandCode: data.BrandCode,
                State: data.State,
                Status: data.Status,
                CreatedOn: data.CreatedOn,
                UpdateOn: data.UpdateOn,
                StartOn: data.StartOn,
                EndOn: data.EndOn,
                ExpireOn: data.ExpireOn);
    }

    public async ValueTask InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        await GameProviderId_IndexAsync(
            collection: collection,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await GameProviderCode_IndexAsync(
            collection: collection,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await BrandId_IndexAsync(
            collection: collection,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await BrandCode_IndexAsync(
            collection: collection,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await State_IndexAsync(
            collection: collection,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await CreatedOn_IndexAsync(
            collection: collection,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await ExpireOn_IndexAsync(
            collection: collection,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<BrandForm> QueryAsync(
        Guid brandId,
        BrandFormState? state,
        int pageSize = 100,
       [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var filter = QueryFilter(
            brandId,
            state);

        var sort = Builders<BrandFormModel>
            .Sort
            .Ascending(x => x.Id);

        var options = new FindOptions<BrandFormModel>
        {
            Sort = sort,
            Limit = pageSize,
        };
        var asyncCursor = await collection
                    .FindAsync(
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
                yield return new BrandForm(
                    current.Id,
                    current.GameProviderId,
                    current.GameProviderCode,
                    current.BrandId,
                    current.BrandCode,
                    current.State,
                    current.Status,
                    current.CreatedOn,
                    current.UpdateOn,
                    current.StartOn,
                    current.EndOn,
                    current.ExpireOn);
            }
        }
    }

    public async ValueTask UpdateCompleteAsync(
        long id,
        string status = "success",
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var builder = Builders<BrandFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<BrandFormModel>.Update
            .Set(s => s.State, BrandFormState.Complete)
            .Set(s => s.Status, status)
            .Set(s => s.EndOn, DateTime.UtcNow)
            .Set(s => s.UpdateOn, DateTime.UtcNow)
            .Set(s => s.ExpireOn, DateTime.UtcNow.AddDays(7));

        _ = await collection
             .UpdateOneAsync(
                filter: filter,
                update: update,
                cancellationToken: cancellationToken)
             .ConfigureAwait(false);
    }

    public async ValueTask UpdateFailedAsync(
        long id,
        string status = "fail",
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var builder = Builders<BrandFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<BrandFormModel>.Update
            .Set(s => s.State, BrandFormState.Failed)
            .Set(s => s.Status, status)
            .Set(s => s.EndOn, DateTime.UtcNow)
            .Set(s => s.UpdateOn, DateTime.UtcNow)
            .Set(s => s.ExpireOn, DateTime.UtcNow.AddDays(7));

        _ = await collection
             .UpdateOneAsync(
                filter: filter,
                update: update,
                cancellationToken: cancellationToken)
             .ConfigureAwait(false);
    }

    public async ValueTask UpdateProcessingAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var builder = Builders<BrandFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<BrandFormModel>.Update
            .Set(s => s.State, BrandFormState.Processing)
            .Set(s => s.Status, $"{BrandFormState.Processing}...")
            .Set(s => s.StartOn, DateTime.UtcNow)
            .Set(s => s.UpdateOn, DateTime.UtcNow);

        _ = await collection
        .UpdateOneAsync(
            filter: filter,
            update: update,
            cancellationToken: cancellationToken)
        .ConfigureAwait(false);
    }

    private static Task BrandCode_IndexAsync(
        IMongoCollection<BrandFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<BrandFormModel>.IndexKeys
            .Ascending(m => m.BrandCode);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_brand_code"
        };
        var createIndexModel = new CreateIndexModel<BrandFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task BrandId_IndexAsync(
        IMongoCollection<BrandFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<BrandFormModel>.IndexKeys
            .Ascending(m => m.BrandId);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_brand_id"
        };
        var createIndexModel = new CreateIndexModel<BrandFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task CreatedOn_IndexAsync(
        IMongoCollection<BrandFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<BrandFormModel>.IndexKeys
            .Ascending(m => m.CreatedOn);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_created_on"
        };
        var createIndexModel = new CreateIndexModel<BrandFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task ExpireOn_IndexAsync(
        IMongoCollection<BrandFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<BrandFormModel>.IndexKeys
            .Ascending(m => m.ExpireOn);

        var createIndexOptions = new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.FromSeconds(1),
            Name = $"ix_expire_on"
        };
        var createIndexModel = new CreateIndexModel<BrandFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task GameProviderCode_IndexAsync(
        IMongoCollection<BrandFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<BrandFormModel>.IndexKeys
            .Ascending(m => m.GameProviderCode);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_game_provider_code"
        };
        var createIndexModel = new CreateIndexModel<BrandFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task GameProviderId_IndexAsync(
        IMongoCollection<BrandFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<BrandFormModel>.IndexKeys
            .Ascending(m => m.GameProviderId);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_game_provider_id"
        };
        var createIndexModel = new CreateIndexModel<BrandFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static FilterDefinition<BrandFormModel> QueryFilter(
        Guid brandId,
        BrandFormState? state)
    {
        var builder = Builders<BrandFormModel>.Filter;

        var filter = builder.Eq(x => x.BrandId, brandId);
        if (state.HasValue)
        {
            filter &= builder.Eq(x => x.State, state);
        }

        return filter;
    }

    private static Task State_IndexAsync(
        IMongoCollection<BrandFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<BrandFormModel>.IndexKeys
            .Ascending(m => m.State);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_state"
        };
        var createIndexModel = new CreateIndexModel<BrandFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private IMongoCollection<BrandFormModel> GetCollection()
        => _database.GetCollection<BrandFormModel>("brand_forms");
}
