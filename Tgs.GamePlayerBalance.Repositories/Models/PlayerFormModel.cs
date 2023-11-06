using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories.Models;

[Table("player_forms")]
public record PlayerFormModel
{
    /// <summary>
    /// The identifier
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.Int64)]
    public long Id { get; set; } = default!;
    [BsonElement("expansion_id")]
    [BsonRepresentation(BsonType.Int64)]
    public long ExpansionFormId { get; init; } = default!;
    /// <summary>
    /// The source
    /// </summary>
    [BsonElement("source")]
    [BsonRepresentation(BsonType.String)]
    public PlayerFormSource Source { get; set; }
    /// <summary>
    /// The game provider identifier
    /// </summary>
    [BsonElement("game_provider_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid GameProviderId { get; set; } = default!;
    /// <summary>
    /// The game provider code
    /// </summary>
    [BsonElement("game_provider_code")]
    public string GameProviderCode { get; set; } = default!;
    /// <summary>
    /// The brand identifier
    /// </summary>
    [BsonElement("brand_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid BrandId { get; set; }
    /// <summary>
    /// The brand code
    /// </summary>
    [BsonElement("brand_code")]
    public string BrandCode { get; set; } = default!;
    /// <summary>
    /// The player code
    /// </summary>
    [BsonElement("player_code")]
    public string PlayerCode { get; set; } = default!;
    /// <summary>
    /// The username
    /// </summary>
    [BsonElement("username")]
    public string Username { get; set; } = default!;
    /// <summary>
    /// The wallet identifier
    /// </summary>
    [BsonElement("wallet_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid WalletId { get; set; } = default!;
    /// <summary>
    /// The currency code
    /// </summary>
    [BsonElement("currency_code")]
    public string CurrencyCode { get; set; } = default!;
    /// <summary>
    /// The state
    /// </summary>
    [BsonElement("state")]
    [BsonRepresentation(BsonType.String)]
    public PlayerFormState State { get; set; }
    /// <summary>
    /// The status
    /// </summary>
    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    [BsonIgnoreIfNull]
    public string Status { get; set; } = default!;
    /// <summary>
    /// The created on
    /// </summary>
    [BsonElement("created_on")]
    public DateTime CreatedOn { get; set; }
    /// <summary>
    /// The update on
    /// </summary>
    [BsonElement("update_on")]
    public DateTime UpdateOn { get; set; }
    /// <summary>
    /// The start on
    /// </summary>
    [BsonIgnoreIfNull]
    [BsonElement("start_on")]
    public DateTime? StartOn { get; set; }
    /// <summary>
    /// The end on
    /// </summary>
    [BsonIgnoreIfNull]
    [BsonElement("end_on")]
    public DateTime? EndOn { get; set; }
    /// <summary>
    /// The expire on
    /// </summary>
    [BsonElement("expire_on")]
    [BsonIgnoreIfNull]
    public DateTime? ExpireOn { get; set; }
    /// <summary>
    /// The fund message
    /// </summary>
    [BsonElement("result")]
    [BsonIgnoreIfNull]
    public PlayerFormFundResultModel? Result { get; set; }
}
