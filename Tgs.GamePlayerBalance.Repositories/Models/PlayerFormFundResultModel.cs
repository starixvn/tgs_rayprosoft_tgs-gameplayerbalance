using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tgs.GameFundTransfer.Protos;

namespace Tgs.GamePlayerBalance.Repositories.Models;

public record PlayerFormFundResultModel
{
	[BsonElement("fund_transaction_id")]
	[BsonRepresentation(BsonType.String)]
	public Guid FundTransactionId { get; init; }
	[BsonElement("cash_transaction_id")]
	[BsonRepresentation(BsonType.String)]
	public string CashTransactionId { get; init; } = default!;
	[BsonElement("cash_resp_transaction_id")]
	[BsonRepresentation(BsonType.String)]
	public Guid CashRespTransactionId { get; init; }
	[BsonElement("game_transaction_id")]
	public string GameTransactionId { get; init; } = default!;
	[BsonElement("game_resp_transaction_id")]
	public string GameRespTransactionId { get; init; } = default!;
}
