namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerBalanceReslut(
    decimal Balance,
    decimal FreeBalance,
    decimal OutstandingBalance);
