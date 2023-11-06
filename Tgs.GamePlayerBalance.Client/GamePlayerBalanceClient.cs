using Google.Protobuf;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Client
{
    public class GamePlayerBalanceClient : IGamePlayerBalanceClient
    {
        private readonly IMessageSender _messageSender;

        public GamePlayerBalanceClient(
            IMessageSender messageSender)
        {
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        }

        public ValueTask DeletePlayerBalanceAsync(string gameProviderCode, string playerCode, CancellationToken cancellationToken = default)
        {
            var request = new PlayerBalanceRequest
            {
                PlayerCode = playerCode,
                GameProviderCode = gameProviderCode,
            };

            return _messageSender.PublishAsync(
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
            var data = await _messageSender.RequestAsync(
                subject: NatsConst.GamePlayerBalancePlayerBalance,
                data: request.ToByteArray(),
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return PlayerBalanceReply.Parser.ParseFrom(data.Span)
                ?? throw new NullReferenceException("The request data resolves to a null value");
        }
    }
}
