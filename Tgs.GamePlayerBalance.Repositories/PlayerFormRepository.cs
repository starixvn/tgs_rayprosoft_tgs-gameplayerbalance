using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Tgs.GameFundTransfer.Protos;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GamePlayerBalance.Repositories.Models;

namespace Tgs.GamePlayerBalance.Repositories;

internal class PlayerFormRepository : IPlayerFormRepository
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<PlayerFormRepository> _logger;
    private readonly ISnowflake _snowflake;

    public PlayerFormRepository(
        ILogger<PlayerFormRepository> logger,
        IMongoDatabase database,
        ISnowflake snowflake)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _snowflake = snowflake ?? throw new ArgumentNullException(nameof(snowflake));
    }

    public async ValueTask<long> AddAsync(
        Guid gameProviderId,
        string gameProviderCode,
        Guid brandId,
        string brandCode,
        string playerCode,
        string username,
        Guid walletId,
        string currencyCode,
        CancellationToken cancellationToken = default)

    {
        var id = _snowflake.NextId();

        var collection = GetCollection();

        var document = new PlayerFormModel
        {
            Id = id,
            ExpansionFormId = id,
            Source = PlayerFormSource.Player,
            GameProviderId = gameProviderId,
            GameProviderCode = gameProviderCode,
            BrandId = brandId,
            BrandCode = brandCode,
            PlayerCode = playerCode,
            Username = username,
            WalletId = walletId,
            CurrencyCode = currencyCode,
            State = PlayerFormState.Queued,
            Status = $"{PlayerFormState.Queued}...",
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

    public async ValueTask<long?> BrandGetFirstIdAsync(
        long expansionFormId,
        PlayerFormState state,
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var builder = Builders<PlayerFormModel>.Filter;
        var sort = Builders<PlayerFormModel>.Sort.Ascending(x => x.Id);
        var filter = builder.Eq(x => x.ExpansionFormId, expansionFormId) &
                    builder.Eq(x => x.Source, PlayerFormSource.Brand) &
                    builder.Eq(x => x.State, state);

        var options = new FindOptions<PlayerFormModel>
        {
            Limit = 1,
            Sort = sort
        };
        var asyncCursor = await collection
                    .FindAsync(
                        filter: filter,
                        options: options,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

        return (await collection
            .FindAsync(
                filter: filter,
                options: options,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false))
            .FirstOrDefault(
            cancellationToken: cancellationToken)?.Id;
    }

    public async IAsyncEnumerable<long> BrandPlayerFormAddAsync(
        IEnumerable<PlayerFormAdd> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var documents = source.Select(x => new PlayerFormModel
        {
            Id = x.FormId,
            ExpansionFormId = x.ExpansionFormId,
            Source = PlayerFormSource.Brand,
            GameProviderId = x.GameProviderId,
            GameProviderCode = x.GameProviderCode,
            BrandId = x.BrandId,
            BrandCode = x.BrandCode,
            PlayerCode = x.PlayerCode,
            Username = x.Username,
            WalletId = x.WalletId,
            CurrencyCode = x.CurrencyCode,
            State = PlayerFormState.Queued,
            Status = $"{PlayerFormState.Queued}...",
            CreatedOn = DateTime.UtcNow,
            UpdateOn = DateTime.UtcNow,
            ExpireOn = null
        });

        var options = new InsertManyOptions
        {
            //Optional. If true, perform an ordered insert of the documents in the array,
            //and if an error occurs with one of documents,
            //MongoDB will return without processing the remaining documents in the array.
            //
            //If false, perform an unordered insert,
            //and if an error occurs with one of documents,
            //continue processing the remaining documents in the array.
            //
            //Defaults to true.
            IsOrdered = false,
        };

        try
        {
            await collection
                 .InsertManyAsync(
                     documents: documents,
                     options: options,
                     cancellationToken: cancellationToken)
                 .ConfigureAwait(false);
        }
        catch (MongoBulkWriteException mongoBulkWrite)
        {
            _logger.LogError(mongoBulkWrite, "Batch add error occurred");

            foreach (var item in source)
            {
                if (mongoBulkWrite.WriteErrors.Where(x => x.Message.Contains(item.PlayerCode, StringComparison.OrdinalIgnoreCase)).Any())
                {
                    _logger.LogError("remove existing players.({playerCode})", item.PlayerCode);

                    documents = documents.Where(x => !x.PlayerCode.Equals(item.PlayerCode));
                }
            }
        }

        if (!documents.Any())
        {
            yield break;
        }

        foreach (var document in documents)
        {
            yield return document.Id;
        }
    }

    public async IAsyncEnumerable<long> BrandQueryIdAsync(
        long expansionFormId,
        PlayerFormState state,
        long id, int pageSize = 1000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var builder = Builders<PlayerFormModel>.Filter;
        var sort = Builders<PlayerFormModel>.Sort.Ascending(x => x.Id);
        var filter = builder.Eq(x => x.ExpansionFormId, expansionFormId) &
                    builder.Eq(x => x.Source, PlayerFormSource.Brand) &
                    builder.Eq(x => x.State, state) &
                    builder.Gt(x => x.Id, id);

        var options = new FindOptions<PlayerFormModel>
        {
            Limit = pageSize,
            Sort = sort
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
                yield return current.Id;
            }
        }
    }

    public async ValueTask<PlayerForm?> GetAsync(
        long id,
        CancellationToken cancellationToken = default)

    {
        var collection = GetCollection();

        var builder = Builders<PlayerFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var options = new FindOptions<PlayerFormModel>
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
            : new PlayerForm(
                Id: data.Id,
                ExpansionFormId: data.ExpansionFormId,
                GameProviderId: data.GameProviderId,
                GameProviderCode: data.GameProviderCode,
                BrandId: data.BrandId,
                BrandCode: data.BrandCode,
                PlayerCode: data.PlayerCode,
                Username: data.Username,
                WalletId: data.WalletId,
                CurrencyCode: data.CurrencyCode,
                State: data.State,
                Status: data.Status,
                CreatedOn: data.CreatedOn,
                UpdateOn: data.UpdateOn,
                StartOn: data.StartOn,
                EndOn: data.EndOn,
                ExpireOn: data.ExpireOn,
                Result: data.Result == null
                ? null
                : new PlayerFormFundResult(
                    data.Result.State,
                    data.Result.Message,
                    data.Result.FundTransactionId,
                    data.Result.TransactionId,
                    data.Result.ExternalTransactionId));
    }

    public async ValueTask<long?> ImportGetFirstIdAsync(
        long expansionFormId,
        PlayerFormState state,
        CancellationToken cancellationToken = default)

    {
        var collection = GetCollection();

        var builder = Builders<PlayerFormModel>.Filter;
        var sort = Builders<PlayerFormModel>.Sort.Ascending(x => x.Id);
        var filter = builder.Eq(x => x.ExpansionFormId, expansionFormId) &
                    builder.Eq(x => x.Source, PlayerFormSource.Import) &
                    builder.Eq(x => x.State, state);

        var options = new FindOptions<PlayerFormModel>
        {
            Limit = 1,
            Sort = sort
        };
        var asyncCursor = await collection
                    .FindAsync(
                        filter: filter,
                        options: options,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

        return (await collection
            .FindAsync(
                filter: filter,
                options: options,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false))
            .FirstOrDefault(
            cancellationToken: cancellationToken)?.Id;
    }

    public async IAsyncEnumerable<long> ImportPlayerFormAddAsync(
        IEnumerable<PlayerFormAdd> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)

    {
        var collection = GetCollection();

        var documents = source.Select(x => new PlayerFormModel
        {
            Id = x.FormId,
            ExpansionFormId = x.ExpansionFormId,
            Source = PlayerFormSource.Import,
            GameProviderId = x.GameProviderId,
            GameProviderCode = x.GameProviderCode,
            BrandId = x.BrandId,
            BrandCode = x.BrandCode,
            PlayerCode = x.PlayerCode,
            Username = x.Username,
            WalletId = x.WalletId,
            CurrencyCode = x.CurrencyCode,
            State = PlayerFormState.Queued,
            Status = $"{PlayerFormState.Queued}...",
            CreatedOn = DateTime.UtcNow,
            UpdateOn = DateTime.UtcNow,
            ExpireOn = null
        });

        var options = new InsertManyOptions
        {
            //Optional. If true, perform an ordered insert of the documents in the array,
            //and if an error occurs with one of documents,
            //MongoDB will return without processing the remaining documents in the array.
            //
            //If false, perform an unordered insert,
            //and if an error occurs with one of documents,
            //continue processing the remaining documents in the array.
            //
            //Defaults to true.
            IsOrdered = false,
        };

        try
        {
            await collection
                 .InsertManyAsync(
                     documents: documents,
                     options: options,
                     cancellationToken: cancellationToken)
                 .ConfigureAwait(false);
        }
        catch (MongoBulkWriteException mongoBulkWrite)
        {
            _logger.LogError(mongoBulkWrite, "Batch add error occurred");

            foreach (var item in source)
            {
                if (mongoBulkWrite.WriteErrors.Where(x => x.Message.Contains(item.PlayerCode, StringComparison.OrdinalIgnoreCase)).Any())
                {
                    _logger.LogError("remove existing players.({playerCode})", item.PlayerCode);

                    documents = documents.Where(x => !x.PlayerCode.Equals(item.PlayerCode));
                }
            }
        }

        if (!documents.Any())
        {
            yield break;
        }

        foreach (var document in documents)
        {
            yield return document.Id;
        }
    }

    public async IAsyncEnumerable<long> ImportQueryIdAsync(
        long expansionFormId,
        PlayerFormState state,
        long id,
        int pageSize = 1000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var builder = Builders<PlayerFormModel>.Filter;
        var sort = Builders<PlayerFormModel>.Sort.Ascending(x => x.Id);
        var filter = builder.Eq(x => x.ExpansionFormId, expansionFormId) &
                    builder.Eq(x => x.Source, PlayerFormSource.Import) &
                    builder.Eq(x => x.State, state) &
                    builder.Gt(x => x.Id, id);

        var options = new FindOptions<PlayerFormModel>
        {
            Limit = pageSize,
            Sort = sort
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
                yield return current.Id;
            }
        }
    }

    public async ValueTask InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        await GameProviderId_IndexAsync(
                collection: collection,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await GameProviderCode_IndexAsync(
                collection: collection,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await Search_IndexAsync(
                collection: collection,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await Search_ExpansionFormId_IndexAsync(
                collection: collection,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await BrandId_IndexAsync(
                collection: collection,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await BrandCode_IndexAsync(
                collection: collection,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await State_IndexAsync(
                collection: collection,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await CreatedOn_IndexAsync(
                collection: collection,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await ExpireOn_IndexAsync(
                collection: collection,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async IAsyncEnumerable<PlayerForm> QueryAsync(
        string playerCode,
        int pageSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();


        var builder = Builders<PlayerFormModel>.Filter;

        var filter = builder.Eq(x => x.PlayerCode, playerCode);

        var sort = Builders<PlayerFormModel>
            .Sort
            .Ascending(x => x.Id);

        var options = new FindOptions<PlayerFormModel>
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
                yield return new PlayerForm(
                    Id: current.Id,
                    ExpansionFormId: current.ExpansionFormId,
                    GameProviderId: current.GameProviderId,
                    GameProviderCode: current.GameProviderCode,
                    BrandId: current.BrandId,
                    BrandCode: current.BrandCode,
                    PlayerCode: current.PlayerCode,
                    Username: current.Username,
                    WalletId: current.WalletId,
                    CurrencyCode: current.CurrencyCode,
                    State: current.State,
                    Status: current.Status,
                    CreatedOn: current.CreatedOn,
                    UpdateOn: current.UpdateOn,
                    StartOn: current.StartOn,
                    EndOn: current.EndOn,
                    ExpireOn: current.ExpireOn,
                    Result: current.Result == null
                    ? null
                    : new PlayerFormFundResult(
                        current.Result.State,
                        current.Result.Message,
                        current.Result.FundTransactionId,
                        current.Result.TransactionId,
                        current.Result.ExternalTransactionId));
            }
        }
    }

    public async ValueTask UpdateCompleteAsync(
                long id,
        StateCode resultStateCode,
        string resultMessage,
        Guid resultFundTransactionId,
        Guid resultTransactionId,
        string resultExternalTransactionId,
        string status = "success",
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var result = new PlayerFormFundResultModel
        {
            State = resultStateCode,
            Message = resultMessage,
            FundTransactionId = resultFundTransactionId,
            TransactionId = resultTransactionId,
            ExternalTransactionId = resultExternalTransactionId
        };

        var builder = Builders<PlayerFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<PlayerFormModel>.Update
            .Set(s => s.State, PlayerFormState.Complete)
            .Set(s => s.Status, status)
            .Set(s => s.Result, result)
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

    public async ValueTask UpdateCompleteAsync(
        long id,
        string status = "success",
        CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();

        var builder = Builders<PlayerFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<PlayerFormModel>.Update
            .Set(s => s.State, PlayerFormState.Complete)
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

        var builder = Builders<PlayerFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<PlayerFormModel>.Update
            .Set(s => s.State, PlayerFormState.Failed)
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

        var builder = Builders<PlayerFormModel>.Filter;
        var filter = builder.Eq(x => x.Id, id);
        var update = Builders<PlayerFormModel>.Update
            .Set(s => s.State, PlayerFormState.Processing)
            .Set(s => s.Status, $"{PlayerFormState.Processing}...")
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
        IMongoCollection<PlayerFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerFormModel>.IndexKeys
            .Ascending(m => m.BrandCode);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_brand_code"
        };
        var createIndexModel = new CreateIndexModel<PlayerFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task BrandId_IndexAsync(
        IMongoCollection<PlayerFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerFormModel>.IndexKeys
            .Ascending(m => m.BrandId);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_brand_id"
        };
        var createIndexModel = new CreateIndexModel<PlayerFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task CreatedOn_IndexAsync(
        IMongoCollection<PlayerFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerFormModel>.IndexKeys
            .Ascending(m => m.CreatedOn);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_created_on"
        };
        var createIndexModel = new CreateIndexModel<PlayerFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task ExpireOn_IndexAsync(
        IMongoCollection<PlayerFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerFormModel>.IndexKeys
            .Ascending(m => m.ExpireOn);

        var createIndexOptions = new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.FromSeconds(1),
            Name = $"ix_expire_on"
        };
        var createIndexModel = new CreateIndexModel<PlayerFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task GameProviderCode_IndexAsync(
        IMongoCollection<PlayerFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerFormModel>.IndexKeys
            .Ascending(m => m.GameProviderCode);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_game_provider_code"
        };
        var createIndexModel = new CreateIndexModel<PlayerFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task GameProviderId_IndexAsync(
        IMongoCollection<PlayerFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerFormModel>.IndexKeys
            .Ascending(m => m.GameProviderId);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_game_provider_id"
        };
        var createIndexModel = new CreateIndexModel<PlayerFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task Search_ExpansionFormId_IndexAsync(
        IMongoCollection<PlayerFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerFormModel>.IndexKeys
            .Ascending(m => m.ExpansionFormId)
            .Ascending(m => m.Source)
            .Ascending(m => m.State);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_search_expansion_id"
        };
        var createIndexModel = new CreateIndexModel<PlayerFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task Search_IndexAsync(
        IMongoCollection<PlayerFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerFormModel>.IndexKeys
            .Ascending(m => m.ExpansionFormId)
            .Ascending(m => m.GameProviderCode)
            .Ascending(m => m.BrandCode)
            .Ascending(m => m.PlayerCode);

        var createIndexOptions = new CreateIndexOptions
        {
            Unique = true,
            Name = $"ix_search"
        };
        var createIndexModel = new CreateIndexModel<PlayerFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private static Task State_IndexAsync(
        IMongoCollection<PlayerFormModel> collection,
        CancellationToken cancellationToken = default)
    {
        var index = Builders<PlayerFormModel>.IndexKeys
            .Ascending(m => m.State);

        var createIndexOptions = new CreateIndexOptions
        {
            Name = $"ix_state"
        };
        var createIndexModel = new CreateIndexModel<PlayerFormModel>(index, createIndexOptions);

        return collection.Indexes.CreateOneAsync(
            model: createIndexModel,
            cancellationToken: cancellationToken);
    }

    private IMongoCollection<PlayerFormModel> GetCollection()
        => _database.GetCollection<PlayerFormModel>("player_forms");
}
