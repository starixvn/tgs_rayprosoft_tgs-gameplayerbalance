using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerBalanceDeleteCommandService : ICommandService<PlayerBalanceDeleteCommand>
{
    private readonly IPlayerTempBalanceRepository _repository;

    public PlayerBalanceDeleteCommandService(IPlayerTempBalanceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ValueTask ExecuteAsync(PlayerBalanceDeleteCommand command, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(
            gameProviderCode: command.GameProviderCode,
            playerCode: command.PlayerCode,
            cancellationToken: cancellationToken);
}
