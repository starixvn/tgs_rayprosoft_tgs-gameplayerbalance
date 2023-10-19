using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class ImportFormProcessingMessageHandler : IMessageHandler
{
    private readonly ILogger<ImportFormProcessingMessageHandler> _logger;
    private readonly ICommandService<ImportFormProcessingCommand> _service;

    public ImportFormProcessingMessageHandler(
        ILogger<ImportFormProcessingMessageHandler> logger,
        ICommandService<ImportFormProcessingCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = ImportFormModelRequest.Parser.ParseFrom(data.Span) ??
            throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogInformation("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);

        var command = new ImportFormProcessingCommand(
            Id: request.Id);

        return _service
            .ExecuteAsync(
                command: command,
                cancellationToken: cancellationToken);
    }
}
