using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class BrandFormByAwcAwsQueuedMessageHandler : IMessageHandler
{
    private readonly ILogger<BrandFormByAwcAwsQueuedMessageHandler> _logger;
    private readonly ICommandService<BrandFormByAwcAwsQueuedCommand> _service;

    public BrandFormByAwcAwsQueuedMessageHandler(
        ILogger<BrandFormByAwcAwsQueuedMessageHandler> logger,
        ICommandService<BrandFormByAwcAwsQueuedCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = BrandFormModelRequest.Parser.ParseFrom(data.Span) ??
            throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogInformation("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);

        var command = new BrandFormByAwcAwsQueuedCommand(
            Id: request.Id);

        return _service
            .ExecuteAsync(
                command: command,
                cancellationToken: cancellationToken);
    }
}
