namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerFormAddCommand(
    string GameProviderCode,
    string PlayerCode);
