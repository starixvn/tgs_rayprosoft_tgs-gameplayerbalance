using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Tgs.GamePlayerBalance;

internal class MongoDBConnectionManager
{
    public IMongoDatabase Database { get; }

    public MongoDBConnectionManager(
        IOptions<MongoDBOptions> optionsAccessor)
    {
        if (optionsAccessor is null)
            throw new ArgumentNullException(nameof(optionsAccessor));

        var options = optionsAccessor.Value;

        var mongoUrl = new MongoUrl(options.ConnectionString);
        var _mongoClient = new MongoClient(mongoUrl);
        Database = _mongoClient.GetDatabase("gameplayerbalance");
    }
}
