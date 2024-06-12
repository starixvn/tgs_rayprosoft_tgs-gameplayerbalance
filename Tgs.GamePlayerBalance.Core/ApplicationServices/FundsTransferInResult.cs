using Tgs.GameFundTransfer.Protos;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record FundsTransferInResult(
	Guid FundTransactionId,
	Guid TransactionId,
	string ExternalOrderId);
