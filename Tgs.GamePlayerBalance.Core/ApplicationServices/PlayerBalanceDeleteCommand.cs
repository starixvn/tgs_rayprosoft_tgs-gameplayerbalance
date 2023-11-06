namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerBalanceDeleteCommand(
    string GameProviderCode,
    string PlayerCode);
