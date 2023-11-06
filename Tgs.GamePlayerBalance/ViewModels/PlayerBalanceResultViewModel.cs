namespace Tgs.GamePlayerBalance.ViewModels;

public record PlayerBalanceResultViewModel(
    decimal Balance,
    decimal FreeBalance,
    decimal OutstandingBalance);
