namespace Tgs.GamePlayerBalance.Core.HandleWorkServices;

public record PlayerHadleWorkCommand(
    string GameProviderCode,
    string PlayerCode);
