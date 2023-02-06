using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.HandleWorkServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class BrandAwcAwsMessageHandler : IMessageHandler
{
    private readonly ILogger<BrandAwcAelottoMessageHandler> _logger;
    private readonly ICommandService<AwcAwsHadleWorkCommand> _service;

    public BrandAwcAwsMessageHandler(
        ILogger<BrandAwcAelottoMessageHandler> logger,
        ICommandService<AwcAwsHadleWorkCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }
    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = BrandAwcAwsMessageRequest.Parser.ParseFrom(data.Span) ??
            throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogInformation("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);

        var command = new AwcAwsHadleWorkCommand(
            GameProviderCode: request.GameProviderCode,
            BrandCode: request.BrandCode);

        return _service.ExecuteAsync(command);
    }
}
