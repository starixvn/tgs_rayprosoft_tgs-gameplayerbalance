using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tgs.GamePlayerBalance.Repositories.Models;

public record class BrandTimeFormPlayerModel
{
	[BsonElement("player_code")]
	public string PlayerCode { get; set; } = default!;
}
