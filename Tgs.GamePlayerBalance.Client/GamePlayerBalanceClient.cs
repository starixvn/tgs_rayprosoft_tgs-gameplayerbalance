using Google.Protobuf;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Client;

public class GamePlayerBalanceClient(
    IMessageSender messageSender) : IGamePlayerBalanceClient
{
    public ValueTask DeletePlayerBalanceAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default)
    {
        var request = new PlayerBalanceRequest
        {
            PlayerCode = playerCode,
            GameProviderCode = gameProviderCode,
        };

        return messageSender.PublishAsync(
            subject: NatsConst.GamePlayerBalancePlayerBalance,
            data: request.ToByteArray(),
            cancellationToken: cancellationToken);
    }

    public async ValueTask<PlayerBalanceReply> GetPlayerBalanceAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default)
    {
        var request = new PlayerBalanceRequest
        {
            PlayerCode = playerCode,
            GameProviderCode = gameProviderCode,
        };
        var data = await messageSender.RequestAsync(
            subject: NatsConst.GamePlayerBalancePlayerBalance,
            data: request.ToByteArray(),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return PlayerBalanceReply.Parser.ParseFrom(data.Span)
            ?? throw new NullReferenceException("The request data resolves to a null value");
    }
}
