using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class BrandFormProcessingMessageHandler : IMessageHandler<ReadOnlyMemory<byte>>
{
    private readonly ILogger<BrandFormProcessingMessageHandler> _logger;
    private readonly ICommandService<BrandFormProcessingCommand> _service;

    public BrandFormProcessingMessageHandler(
        ILogger<BrandFormProcessingMessageHandler> logger,
        ICommandService<BrandFormProcessingCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
    {
        var request = BrandFormModelRequest.Parser.ParseFrom(data.Span) ??
            throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogDebug("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);

        var command = new BrandFormProcessingCommand(
            Id: request.Id);

        return _service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}
