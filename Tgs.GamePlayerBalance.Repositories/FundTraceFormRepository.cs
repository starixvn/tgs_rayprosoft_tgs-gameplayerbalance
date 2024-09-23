using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Core.Extensions;
using Tgs.GamePlayerBalance.Core.Models;
using Tgs.GamePlayerBalance.Repositories.Models;

namespace Tgs.GamePlayerBalance.Repositories;

internal class FundTraceFormRepository(
	ILogger<FundTraceFormRepository> logger,
	IMongoDatabase database,
	ISnowflakeWork snowflake) : IFundTraceFormRepository
{
	public async ValueTask<long> AddAsync(string gameProviderCode, IEnumerable<string> players, CancellationToken cancellationToken = default)
	{
		var id = snowflake.GetId();

		var collection = GetCollection();

		var formPlayers = players.Select(player => new FundTraceFormPlayerModel
		{
			PlayerCode = player
		});

		var document = new FundTraceFormModel
		{
			Id = id,
			GameProviderCode = gameProviderCode,
			State = FundTraceFormState.Queued,
			Status = $"{FundTraceFormState.Queued}...",
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

	public async ValueTask<FundTraceForm?> GetAsync(long id, CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();

		var builder = Builders<FundTraceFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var options = new FindOptions<FundTraceFormModel>
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
			: new FundTraceForm(
				Id: data.Id,
				GameProviderCode: data.GameProviderCode,
				State: data.State,
				Status: data.Status,
				Players: data.Players.Select(x => new FundTraceFormPlayer(x.PlayerCode)),
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

		var builder = Builders<FundTraceFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var update = Builders<FundTraceFormModel>.Update
			.Set(s => s.State, FundTraceFormState.Complete)
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

		var builder = Builders<FundTraceFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var update = Builders<FundTraceFormModel>.Update
			.Set(s => s.State, FundTraceFormState.Failed)
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

		var builder = Builders<FundTraceFormModel>.Filter;
		var filter = builder.Eq(x => x.Id, id);
		var update = Builders<FundTraceFormModel>.Update
			.Set(s => s.State, FundTraceFormState.Processing)
			.Set(s => s.Status, $"{FundTraceFormState.Processing}...")
			.Set(s => s.StartOn, DateTime.UtcNow)
			.Set(s => s.UpdateOn, DateTime.UtcNow);

		_ = await collection.UpdateOneAsync(
			filter: filter,
			update: update,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task CreatedOn_IndexAsync(IMongoCollection<FundTraceFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<FundTraceFormModel>.IndexKeys
			.Ascending(m => m.CreatedOn);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_created_on"
		};
		var createIndexModel = new CreateIndexModel<FundTraceFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task ExpireOn_IndexAsync(IMongoCollection<FundTraceFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<FundTraceFormModel>.IndexKeys
			.Ascending(m => m.ExpireOn);

		var createIndexOptions = new CreateIndexOptions
		{
			ExpireAfter = TimeSpan.FromSeconds(1),
			Name = $"ix_expire_on"
		};
		var createIndexModel = new CreateIndexModel<FundTraceFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task GameProviderCode_IndexAsync(IMongoCollection<FundTraceFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<FundTraceFormModel>.IndexKeys
			.Ascending(m => m.GameProviderCode);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_game_provider_code"
		};
		var createIndexModel = new CreateIndexModel<FundTraceFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static async Task State_IndexAsync(IMongoCollection<FundTraceFormModel> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<FundTraceFormModel>.IndexKeys
			.Ascending(m => m.State);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = $"ix_state"
		};
		var createIndexModel = new CreateIndexModel<FundTraceFormModel>(index, createIndexOptions);

		_ = await collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private IMongoCollection<FundTraceFormModel> GetCollection()
		=> database.GetCollection<FundTraceFormModel>(typeof(FundTraceFormModel).GetTableName());
}
