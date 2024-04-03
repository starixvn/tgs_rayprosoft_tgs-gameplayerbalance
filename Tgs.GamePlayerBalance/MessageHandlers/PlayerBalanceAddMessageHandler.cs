using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class PlayerBalanceAddMessageHandler : IMessageHandler<ReadOnlyMemory<byte>>
{
    private readonly ILogger<PlayerBalanceAddMessageHandler> _logger;
    private readonly ICommandService<PlayerBalanceAddCommand> _service;

    public PlayerBalanceAddMessageHandler(
        ILogger<PlayerBalanceAddMessageHandler> logger,
        ICommandService<PlayerBalanceAddCommand> service)
    {
        _logger = logger;
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
    {
        var request = PlayerBalanceAddRequest.Parser.ParseFrom(data.Span) ??
            throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogDebug("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);

        var command = new PlayerBalanceAddCommand(
            GameProviderCode: request.GameProviderCode,
            PlayerCode: request.PlayerCode,
            Balance: request.Balance.ToDecimal().GetValueOrDefault(0),
            FreeBalance: request.FreeBalance.ToDecimal().GetValueOrDefault(0),
            OutstandingBalance: request.OutstandingBalance.ToDecimal().GetValueOrDefault(0));

        return _service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }

}
