namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerFundResult(
	Guid FundTransactionId,
	Guid TransactionId,
	string ExternalOrderId);
