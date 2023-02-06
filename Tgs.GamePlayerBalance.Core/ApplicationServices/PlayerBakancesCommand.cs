using Tgs.GameCommand.Common;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record PlayerBakancesCommand(
    Guid GameProviderId,
    string GameProviderCode,
    IEnumerable<PlayerInfo> PlayerInfos);
