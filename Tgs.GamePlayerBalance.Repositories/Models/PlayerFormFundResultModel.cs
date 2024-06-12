using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tgs.GameFundTransfer.Protos;

namespace Tgs.GamePlayerBalance.Repositories.Models;

public record PlayerFormFundResultModel
{
	[BsonElement("fund_transaction_id")]
	[BsonRepresentation(BsonType.String)]
	public Guid FundTransactionId { get; init; }
	[BsonElement("transaction_id")]
	[BsonRepresentation(BsonType.String)]
	public Guid TransactionId { get; init; }
	[BsonElement("external_order_id")]
	public string ExternalOrderId { get; init; } = default!;
}
