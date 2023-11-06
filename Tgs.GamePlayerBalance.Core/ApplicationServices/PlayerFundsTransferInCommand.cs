namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerFundsTransferInCommand(
    string GameProviderCode,
    string PlayerCode);
