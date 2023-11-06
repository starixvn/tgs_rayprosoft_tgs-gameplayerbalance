using Tgs.GameFundTransfer.Protos;

namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerFundResult(
    StateCode StateCode,
    string Message,
    Guid FundTransactionId,
    Guid TransactionId,
    string ExternalTransactionId);
