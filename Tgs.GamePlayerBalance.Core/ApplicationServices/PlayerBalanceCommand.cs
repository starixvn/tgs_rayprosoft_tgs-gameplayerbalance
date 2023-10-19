namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerBalanceCommand(
    string GameProviderCode,
    string PlayerCode);
