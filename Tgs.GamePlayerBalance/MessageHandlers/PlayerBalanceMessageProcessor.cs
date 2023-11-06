using Google.Protobuf;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class PlayerBalanceMessageProcessor : IMessageProcessor
{
    private readonly ILogger<PlayerBalanceMessageProcessor> _logger;
    private readonly IGetService<PlayerBalanceQuery, PlayerBalanceReslut> _service;

    public PlayerBalanceMessageProcessor(
        ILogger<PlayerBalanceMessageProcessor> logger,
        IGetService<PlayerBalanceQuery, PlayerBalanceReslut> service)
    {
        _logger = logger;
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = PlayerBalanceRequest.Parser.ParseFrom(data.Span) ??
            throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogDebug("LogOn:{logOn} Information Request:{receive}", DateTime.Now.ToString("o"), request);

        var query = new PlayerBalanceQuery(
            PlayerCode: request.PlayerCode,
            GameProviderCode: request.GameProviderCode);

        var reslut = await _service.GetAsync(
            query: query,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var reply = new PlayerBalanceReply
        {
            Balance = reslut.Balance,
            FreeBalance = reslut.FreeBalance,
            OutstandingBalance = reslut.OutstandingBalance,
        };

        return reply.ToByteArray();
    }
}
