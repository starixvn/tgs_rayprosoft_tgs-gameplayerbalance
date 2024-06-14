namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerFormFundResult(
	Guid FundTransactionId,
	string CashTransactionId,
	Guid CashRespTransactionId,
	string GameTransactionId,
	string GameRespTransactionId);
