namespace Tgs.GamePlayerBalance.Core.Models;

public record PlayerBalance(
	decimal Balance,
	decimal FreeBalance,
	decimal OutstandingBalance);
