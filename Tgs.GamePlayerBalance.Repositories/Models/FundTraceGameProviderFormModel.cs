using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories.Models;

[Table("fundtrace_gameprovider_forms")]
public record FundTraceGameProviderFormModel
{
	[BsonId]
	[BsonRepresentation(BsonType.Int64)]
	public long Id { get; set; } = default!;

	[BsonElement("game_provider_code")]
	public string GameProviderCode { get; set; } = default!;

	[BsonElement("state")]
	[BsonRepresentation(BsonType.String)]
	public GameProviderTimeFormState State { get; set; }

	[BsonElement("status")]
	[BsonRepresentation(BsonType.String)]
	[BsonIgnoreIfNull]
	public string Status { get; set; } = default!;

	[BsonElement("players")]
	[BsonIgnoreIfNull]
	public IEnumerable<FundTraceGameProviderFormPlayerModel> Players { get; set; } = default!;

	[BsonElement("created_on")]
	public DateTime CreatedOn { get; set; }

	[BsonElement("update_on")]
	public DateTime UpdateOn { get; set; }

	[BsonIgnoreIfNull]
	[BsonElement("start_on")]
	public DateTime? StartOn { get; set; }

	[BsonIgnoreIfNull]
	[BsonElement("end_on")]
	public DateTime? EndOn { get; set; }

	[BsonElement("expire_on")]
	[BsonIgnoreIfNull]
	public DateTime? ExpireOn { get; set; }
}
