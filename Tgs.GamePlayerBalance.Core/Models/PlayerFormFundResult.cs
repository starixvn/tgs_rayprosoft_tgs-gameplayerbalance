using Tgs.GameFundTransfer.Protos;

namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerFormFundResult(
	StateCode State,
	string Message,
	Guid FundTransactionId,
	Guid TransactionId,
	string ExternalTransactionId);
