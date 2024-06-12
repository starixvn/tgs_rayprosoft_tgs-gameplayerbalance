using Tgs.GameFundTransfer.Protos;

namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerFormFundResult(
	Guid FundTransactionId,
	Guid TransactionId,
	string ExternalOrderId);
