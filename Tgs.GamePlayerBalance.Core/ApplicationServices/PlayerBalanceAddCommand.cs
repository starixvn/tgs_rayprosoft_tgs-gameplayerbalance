namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerBalanceAddCommand(
    string GameProviderCode,
    string PlayerCode,
    decimal Balance,
    decimal FreeBalance,
    decimal OutstandingBalance);
