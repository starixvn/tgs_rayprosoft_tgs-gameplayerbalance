using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tgs.GamePlayerBalance.Repositories.Models;

[Table("player_temp_balances")]
public record PlayerTempBalanceModel
{
    /// <summary>
    /// The identifier
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; } = default!;
    /// <summary>
    /// The game provider code
    /// </summary>
    [BsonElement("game_provider_code")]
    public string GameProviderCode { get; set; } = default!;
    /// <summary>
    /// The player code
    /// </summary>
    [BsonElement("player_code")]
    public string PlayerCode { get; set; } = default!;
    /// <summary>
    /// The balance
    /// </summary>
    [BsonElement("balance")]
    [BsonRepresentation(BsonType.Decimal128)]
    [BsonDefaultValue(0)]
    public decimal Balance { get; set; }
    /// <summary>
    /// The free balance
    /// </summary>
    [BsonElement("free_balance")]
    [BsonRepresentation(BsonType.Decimal128)]
    [BsonDefaultValue(0)]
    public decimal FreeBalance { get; set; }
    /// <summary>
    /// The outstanding balance
    /// </summary>
    [BsonElement("outstanding_balance")]
    [BsonRepresentation(BsonType.Decimal128)]
    [BsonDefaultValue(0)]
    public decimal OutstandingBalance { get; set; }

    /// <summary>
    /// The created on
    /// </summary>
    [BsonElement("created_on")]
    [BsonIgnoreIfNull]
    public DateTime CreatedOn { get; set; }
    /// <summary>
    /// The expire on
    /// </summary>
    [BsonElement("expire_on")]
    [BsonIgnoreIfNull]
    public DateTime ExpireOn { get; set; }
}
