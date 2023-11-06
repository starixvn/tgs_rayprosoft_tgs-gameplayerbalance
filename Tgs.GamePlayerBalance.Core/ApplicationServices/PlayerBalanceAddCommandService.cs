using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerBalanceAddCommandService : ICommandService<PlayerBalanceAddCommand>
{
    private readonly IPlayerTempBalanceRepository _repository;

    public PlayerBalanceAddCommandService(IPlayerTempBalanceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask ExecuteAsync(PlayerBalanceAddCommand command, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistAsync(
            gameProviderCode: command.GameProviderCode,
            playerCode: command.PlayerCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false))
        {
            await _repository.AddAsync(
                  gameProviderCode: command.GameProviderCode,
                  playerCode: command.PlayerCode,
                  balance: command.Balance,
                  freeBalance: command.FreeBalance,
                  outstandingBalance: command.OutstandingBalance,
                  cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
