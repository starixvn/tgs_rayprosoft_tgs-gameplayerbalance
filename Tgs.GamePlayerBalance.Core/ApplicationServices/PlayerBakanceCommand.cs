namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerBakanceCommand(
    string GameProviderCode,
    string PlayerCode);
