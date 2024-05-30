using System.Runtime.CompilerServices;
using MongoDB.Driver;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Extensions;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GamePlayerBalance.Repositories.Models;

namespace Tgs.GamePlayerBalance.Repositories;

internal class ImportFormRepository(
	IMongoDatabase database,
	ISnowflakeWork snowflake) : IImportFormRepository
{
	public async ValueTask<long> AddAsync(
		Guid gameProviderId,
		string gameProviderCode,
		IEnumerable<string> players,
		CancellationToken cancellationToken = default)

	{
		var id = snowflake.GetId();

		var collection = GetCollection();

		var formPlayers = players.Select(player => new ImportFormPlayerModel
		{
			PlayerCode = player
		});

		var document = new ImportFormModel
		{
			Id = id,
			GameProviderId = gameProviderId,
			GameProviderCode = gameProviderCode,
			State = ImportFormState.Queued,
			Status = $"{ImportFormState.Queued}...",
			Players = formPlayers,
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

	public async ValueTask<ImportForm?> GetAsync(
		long id,
		CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();

		var builder = Builders<ImportFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var options = new FindOptions<ImportFormModel>
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
			: new ImportForm(
				Id: data.Id,
				GameProviderId: data.GameProviderId,
				GameProviderCode: data.GameProviderCode,
				State: data.State,
				Status: data.Status,
				Players: data.Players.Select(x => new ImportFormPlayer(x.PlayerCode)),
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

	public async IAsyncEnumerable<ImportForm> QueryAsync(
		Guid gameProviderId,
		ImportFormState? state,
		int pageSize = 100,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)

	{
		var collection = GetCollection();

		var filter = QueryFilter(
			gameProviderId,
			state);

		var sort = Builders<ImportFormModel>
			.Sort
			.Descending(x => x.Id);

		var options = new FindOptions<ImportFormModel>
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
				yield return new ImportForm(
					Id: current.Id,
					GameProviderId: current.GameProviderId,
					GameProviderCode: current.GameProviderCode,
					State: current.State,
					Status: current.Status,
					Players: current.Players.Select(x => new ImportFormPlayer(x.PlayerCode)),
					CreatedOn: current.CreatedOn,
					UpdateOn: current.UpdateOn,
					StartOn: current.StartOn,
					EndOn: current.EndOn,
					ExpireOn: current.ExpireOn);
			}
		}
	}

	public async ValueTask UpdateCompleteAsync(
		long id,
		string status = "success",
		CancellationToken cancellationToken = default)

	{
		var collection = GetCollection();

		var builder = Builders<ImportFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var update = Builders<ImportFormModel>.Update
			.Set(s => s.State, ImportFormState.Complete)
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

		var builder = Builders<ImportFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var update = Builders<ImportFormModel>.Update
			.Set(s => s.State, ImportFormState.Failed)
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

		var builder = Builders<ImportFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var update = Builders<ImportFormModel>.Update
			.Set(s => s.State, ImportFormState.Processing)
			.Set(s => s.Status, $"{ImportFormState.Processing}...")
			.Set(s => s.StartOn, DateTime.UtcNow)
			.Set(s => s.UpdateOn, DateTime.UtcNow);

		_ = await collection
		.UpdateOneAsync(
			filter: filter,
			update: update,
			cancellationToken: cancellationToken)
		.ConfigureAwait(false);
	}

	private static async Task CreatedOn_IndexAsync(
		IMongoCollection<ImportFormModel> collection,
		CancellationToken cancellationToken = default)
	{
		var index = Builders<ImportFormModel>.IndexKeys
			.Ascending(m => m.CreatedOn);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_created_on"
		};
		var createIndexModel = new CreateIndexModel<ImportFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task ExpireOn_IndexAsync(
		IMongoCollection<ImportFormModel> collection,
		CancellationToken cancellationToken = default)
	{
		var index = Builders<ImportFormModel>.IndexKeys
			.Ascending(m => m.ExpireOn);

		var createIndexOptions = new CreateIndexOptions
		{
			ExpireAfter = TimeSpan.FromSeconds(1),
			Name = $"ix_expire_on"
		};
		var createIndexModel = new CreateIndexModel<ImportFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task GameProviderCode_IndexAsync(
		IMongoCollection<ImportFormModel> collection,
		CancellationToken cancellationToken = default)
	{
		var index = Builders<ImportFormModel>.IndexKeys
			.Ascending(m => m.GameProviderCode);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_game_provider_code"
		};
		var createIndexModel = new CreateIndexModel<ImportFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task GameProviderId_IndexAsync(
		IMongoCollection<ImportFormModel> collection,
		CancellationToken cancellationToken = default)
	{
		var index = Builders<ImportFormModel>.IndexKeys
			.Ascending(m => m.GameProviderId);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_game_provider_id"
		};
		var createIndexModel = new CreateIndexModel<ImportFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static FilterDefinition<ImportFormModel> QueryFilter(
		Guid gameProviderId,
		ImportFormState? state)
	{
		var builder = Builders<ImportFormModel>.Filter;

		var filter = builder.Eq(x => x.GameProviderId, gameProviderId);
		if (state.HasValue)
		{
			filter &= builder.Eq(x => x.State, state);
		}

		return filter;
	}

	private static async Task State_IndexAsync(
		IMongoCollection<ImportFormModel> collection,
		CancellationToken cancellationToken = default)
	{
		var index = Builders<ImportFormModel>.IndexKeys
			.Ascending(m => m.State);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_state"
		};
		var createIndexModel = new CreateIndexModel<ImportFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private IMongoCollection<ImportFormModel> GetCollection()
		=> database.GetCollection<ImportFormModel>(typeof(ImportFormModel).GetTableName());
}
