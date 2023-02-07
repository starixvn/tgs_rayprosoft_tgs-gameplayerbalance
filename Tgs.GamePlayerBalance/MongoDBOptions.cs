namespace Tgs.GamePlayerBalance;

public record MongoDBOptions
{
    public string ConnectionString { get; set; } = "mongodb://localhost";
}
