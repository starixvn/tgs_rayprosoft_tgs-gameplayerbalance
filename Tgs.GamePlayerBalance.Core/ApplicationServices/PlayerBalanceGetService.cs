using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class PlayerBalanceGetService : IGetService<PlayerBalanceQuery, PlayerBalanceReslut>
{
    private readonly IGameProvidersClient _client;
    private readonly IGameProviderRepository _gameProviderRepository;
    private readonly ILogger<PlayerBalanceGetService> _logger;
    private readonly IMessageSender _messageSender;

    public PlayerBalanceGetService(
        ILogger<PlayerBalanceGetService> logger,
        IGameProviderRepository gameProviderRepository,
        IGameProvidersClient client,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameProviderRepository = gameProviderRepository ?? throw new ArgumentNullException(nameof(gameProviderRepository));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask<PlayerBalanceReslut> GetAsync(PlayerBalanceQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("LogOn:{logOn} Information Query:{query}", DateTime.Now.ToString("o"), query);

        var gameProviderId = await _gameProviderRepository.GetIdAsync(
            code: query.GameProviderCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var info = await _gameProviderRepository.GetAsync(
            id: gameProviderId,
            cancellationToken: cancellationToken)
             .ConfigureAwait(false);

        var response = await _client.PlayerBalanceAsync(
            gameProviderCode: info.GameProviderCode,
            playerCode: query.PlayerCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var request = new PlayerBalanceAddRequest
        {
            GameProviderCode = info.GameProviderCode,
            PlayerCode = query.PlayerCode,
            Balance = response.Balance,
            FreeBalance = response.FreeBalance,
            OutstandingBalance = response.OutstandingBalance,
        };

        await _messageSender.PublishAsync(
            NatsConst.GamePlayerBalancePlayerBalanceAdd,
            request.ToByteArray(),
            cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug("LogOn:{logOn} Subject:{subject} Message:This published message.({request})",
            DateTime.Now.ToString("o"),
            NatsConst.GamePlayerBalancePlayerBalanceAdd,
            request);

        return new PlayerBalanceReslut(
            response.Balance,
            response.FreeBalance,
            response.OutstandingBalance);
    }
}
