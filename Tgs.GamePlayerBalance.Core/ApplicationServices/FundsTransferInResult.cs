using Tgs.GameFundTransfer.Protos;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record FundsTransferInResult(
	StateCode StateCode,
	string Message,
	Guid FundTransactionId,
	Guid TransactionId,
	string ExternalTransactionId);
