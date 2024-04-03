using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Common;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.ViewModels;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerController : ControllerBase
{
    /// <summary>
    /// Gets the balance asynchronous.
    /// </summary>
    /// <param name="messageSender">The message sender.</param>
    /// <param name="playerCode">The player code.</param>
    /// <param name="gameProviderCode">The game provider code.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Get /api/player/4ucmqplxb029/KY/balance
    /// </remarks>
    /// <exception cref="System.NullReferenceException">The request data resolves to a null value</exception>
    [HttpGet("{playerCode}/{gameProviderCode}/balance")]
    public async ValueTask<PlayerBalanceResultViewModel> GetBalanceAsync(
        [FromServices] IMessageSender messageSender,
        string playerCode,
        string gameProviderCode)
    {
        var request = new PlayerBalanceRequest
        {
            PlayerCode = playerCode,
            GameProviderCode = gameProviderCode,
        };
        var data = await messageSender.RequestAsync<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(
            subject: NatsConst.GamePlayerBalancePlayerBalance,
            data: request.ToByteArray(),
            cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var reply = PlayerBalanceReply.Parser.ParseFrom(data.Span)
           ?? throw new NullReferenceException("The request data resolves to a null value");

        return new PlayerBalanceResultViewModel(
            reply.Balance.ToDecimal().GetValueOrDefault(0),
            reply.FreeBalance.ToDecimal().GetValueOrDefault(0),
            reply.OutstandingBalance.ToDecimal().GetValueOrDefault(0));
    }
}
