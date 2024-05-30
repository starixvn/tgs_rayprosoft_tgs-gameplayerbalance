using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tgs.GameFundTransfer.Protos;

namespace Tgs.GamePlayerBalance.Repositories.Models;

public record PlayerFormFundResultModel
{
	[BsonElement("state")]
	[BsonRepresentation(BsonType.String)]
	public StateCode State { get; init; }
	[BsonElement("message")]
	public string Message { get; init; } = default!;
	[BsonElement("fund_transaction_id")]
	[BsonRepresentation(BsonType.String)]
	public Guid FundTransactionId { get; init; }
	[BsonElement("transaction_id")]
	[BsonRepresentation(BsonType.String)]
	public Guid TransactionId { get; init; }
	[BsonElement("external_transaction_id")]
	public string ExternalTransactionId { get; init; } = default!;
}
