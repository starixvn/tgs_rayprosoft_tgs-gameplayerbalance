using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Tgs.GamePlayerBalance;

internal class MongoDBConnectionManager
{
	public IMongoDatabase MongoDatabase { get; }

	public MongoDBConnectionManager(
		IOptions<MongoDBOptions> optionsAccessor)
	{
		ArgumentNullException.ThrowIfNull(optionsAccessor);
		var options = optionsAccessor.Value;
		var mongoUrl = new MongoUrl(options.ConnectionString);
		MongoDatabase = new MongoClient(mongoUrl)
		   .GetDatabase("game_player_balances");
	}
}
