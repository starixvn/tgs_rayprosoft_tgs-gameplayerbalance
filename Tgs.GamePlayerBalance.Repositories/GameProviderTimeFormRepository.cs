using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Extensions;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GamePlayerBalance.Repositories.Models;

namespace Tgs.GamePlayerBalance.Repositories;

internal class GameProviderTimeFormRepository(
	ILogger<GameProviderTimeFormRepository> logger,
	IMongoDatabase database,
	ISnowflakeWork snowflake) : IGameProviderTimeFormRepository
{
	public async ValueTask<long> AddAsync(string gameProviderCode, IEnumerable<string> players, CancellationToken cancellationToken = default)
	{
		var id = snowflake.GetId();

		var collection = GetCollection();

		var formPlayers = players.Select(player => new GameProviderTimeFormPlayerModel
		{
			PlayerCode = player
		});

		var document = new GameProviderTimeFormModel
		{
			Id = id,
			GameProviderCode = gameProviderCode,
			State = GameProviderTimeFormState.Queued,
			Status = $"{GameProviderTimeFormState.Queued}...",
			Players = formPlayers,
			CreatedOn = DateTime.UtcNow,
			UpdateOn = DateTime.UtcNow,
			ExpireOn = null
		};

		await collection.InsertOneAsync(
			document: document,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return id;
	}

	public async ValueTask<GameProviderTimeForm?> GetAsync(long id, CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();

		var builder = Builders<GameProviderTimeFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var options = new FindOptions<GameProviderTimeFormModel>
		{
			Skip = 0,
			Limit = 1
		};
		var datas = await collection.FindAsync(
			filter: filter,
			options: options,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var data = await datas.FirstOrDefaultAsync(
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return data == null
			? null
			: new GameProviderTimeForm(
				Id: data.Id,
				GameProviderCode: data.GameProviderCode,
				StartTime: data.StartTime,
				EndTime: data.EndTime,
				State: data.State,
				Status: data.Status,
				Players: data.Players.Select(x => new GameProviderTimeFormPlayer(x.PlayerCode)),
				CreatedOn: data.CreatedOn,
				UpdateOn: data.UpdateOn,
				StartOn: data.StartOn,
				EndOn: data.EndOn,
				ExpireOn: data.ExpireOn);
	}

	public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();

		var tasks = new Task[]{
			Task.Run(async () =>
			{
				try
				{
					await GameProviderCode_IndexAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_Unique");
				}
			}, cancellationToken: cancellationToken),
			Task.Run(async () =>
			{
				try
				{
					await StartTime_IndexAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_StartTime");
				}
			}, cancellationToken: cancellationToken),
			Task.Run(async () =>
			{
				try
				{
					await EndTime_IndexAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_EndTime");
				}
			}, cancellationToken: cancellationToken),
			Task.Run(async () =>
			{
				try
				{
					 await State_IndexAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_State");
				}
			}, cancellationToken: cancellationToken),
			Task.Run(async () =>
			{
				try
				{
					  await CreatedOn_IndexAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_CreatedOn");
				}
			}, cancellationToken: cancellationToken),
			Task.Run(async () =>
			{
				try
				{
					 await ExpireOn_IndexAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_ExpireOn");
				}
			}, cancellationToken: cancellationToken),
		};

		await Task.WhenAll(tasks).ConfigureAwait(false);
	}

	public async ValueTask UpdateCompleteAsync(long id, string status = "success", CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();

		var builder = Builders<GameProviderTimeFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var update = Builders<GameProviderTimeFormModel>.Update
			.Set(s => s.State, GameProviderTimeFormState.Complete)
			.Set(s => s.Status, status)
			.Set(s => s.EndOn, DateTime.UtcNow)
			.Set(s => s.UpdateOn, DateTime.UtcNow)
			.Set(s => s.ExpireOn, DateTime.UtcNow.AddDays(7));

		_ = await collection.UpdateOneAsync(
			filter: filter,
			update: update,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask UpdateFailedAsync(long id, string status = "fail", CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();

		var builder = Builders<GameProviderTimeFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var update = Builders<GameProviderTimeFormModel>.Update
			.Set(s => s.State, GameProviderTimeFormState.Failed)
			.Set(s => s.Status, status)
			.Set(s => s.EndOn, DateTime.UtcNow)
			.Set(s => s.UpdateOn, DateTime.UtcNow)
			.Set(s => s.ExpireOn, DateTime.UtcNow.AddDays(7));

		_ = await collection.UpdateOneAsync(
			filter: filter,
			update: update,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask UpdateProcessingAsync(long id, CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();

		var builder = Builders<GameProviderTimeFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var update = Builders<GameProviderTimeFormModel>.Update
			.Set(s => s.State, GameProviderTimeFormState.Processing)
			.Set(s => s.Status, $"{GameProviderTimeFormState.Processing}...")
			.Set(s => s.StartOn, DateTime.UtcNow)
			.Set(s => s.UpdateOn, DateTime.UtcNow);

		_ = await collection.UpdateOneAsync(
			filter: filter,
			update: update,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task StartTime_IndexAsync(IMongoCollection<GameProviderTimeFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameProviderTimeFormModel>.IndexKeys
			.Ascending(m => m.StartTime);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_start_time"
		};
		var createIndexModel = new CreateIndexModel<GameProviderTimeFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task EndTime_IndexAsync(IMongoCollection<GameProviderTimeFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameProviderTimeFormModel>.IndexKeys
			.Ascending(m => m.EndTime);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_end_time"
		};
		var createIndexModel = new CreateIndexModel<GameProviderTimeFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task CreatedOn_IndexAsync(IMongoCollection<GameProviderTimeFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameProviderTimeFormModel>.IndexKeys
			.Ascending(m => m.CreatedOn);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_created_on"
		};
		var createIndexModel = new CreateIndexModel<GameProviderTimeFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task ExpireOn_IndexAsync(IMongoCollection<GameProviderTimeFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameProviderTimeFormModel>.IndexKeys
			.Ascending(m => m.ExpireOn);

		var createIndexOptions = new CreateIndexOptions
		{
			ExpireAfter = TimeSpan.FromSeconds(1),
			Name = $"ix_expire_on"
		};
		var createIndexModel = new CreateIndexModel<GameProviderTimeFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task GameProviderCode_IndexAsync(IMongoCollection<GameProviderTimeFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameProviderTimeFormModel>.IndexKeys
			.Ascending(m => m.GameProviderCode);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_game_provider_code"
		};
		var createIndexModel = new CreateIndexModel<GameProviderTimeFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task State_IndexAsync(IMongoCollection<GameProviderTimeFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameProviderTimeFormModel>.IndexKeys
			.Ascending(m => m.State);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_state"
		};
		var createIndexModel = new CreateIndexModel<GameProviderTimeFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private IMongoCollection<GameProviderTimeFormModel> GetCollection()
		=> database.GetCollection<GameProviderTimeFormModel>(typeof(GameProviderTimeFormModel).GetTableName());
}
