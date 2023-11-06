using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class PlayerFormQueuedMessageHandler : IMessageHandler
{
    private readonly ILogger<PlayerFormQueuedMessageHandler> _logger;
    private readonly ICommandService<PlayerFormQueuedCommand> _service;

    public PlayerFormQueuedMessageHandler(
        ILogger<PlayerFormQueuedMessageHandler> logger,
        ICommandService<PlayerFormQueuedCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = PlayerFormModelRequest.Parser.ParseFrom(data.Span) ??
            throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogDebug("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);

        var command = new PlayerFormQueuedCommand(
            Id: request.Id);

        return _service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}
