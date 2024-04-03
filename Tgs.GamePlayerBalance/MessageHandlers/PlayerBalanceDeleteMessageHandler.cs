using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class PlayerBalanceDeleteMessageHandler : IMessageHandler<ReadOnlyMemory<byte>>
{
    private readonly ILogger<PlayerBalanceDeleteMessageHandler> _logger;
    private readonly ICommandService<PlayerBalanceDeleteCommand> _service;

    public PlayerBalanceDeleteMessageHandler(
        ILogger<PlayerBalanceDeleteMessageHandler> logger,
        ICommandService<PlayerBalanceDeleteCommand> service)
    {
        _logger = logger;
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
    {
        var request = PlayerBalanceRequest.Parser.ParseFrom(data.Span) ??
            throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogDebug("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);

        var command = new PlayerBalanceDeleteCommand(
            GameProviderCode: request.GameProviderCode,
            PlayerCode: request.PlayerCode);

        return _service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}
