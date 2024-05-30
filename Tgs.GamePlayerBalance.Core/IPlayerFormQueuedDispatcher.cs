using Tgs.GamePlayerBalance.Core.ApplicationServices;

namespace Tgs.GamePlayerBalance.Core;

public interface IPlayerFormQueuedDispatcher
{
	ValueTask PushCommandAsync(PlayerFormQueuedCommand command, CancellationToken cancellationToken = default);
}
