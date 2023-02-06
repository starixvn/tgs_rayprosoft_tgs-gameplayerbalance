using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.GameProviders.Domains;
using Tgs.MessageQueue;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("[controller]")]
[ApiController]
public class BrandsController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public BrandsController(IMessageSender messageSender)
    {
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    [HttpPost("awc-aelotto", Name = "AwcAelotto")]
    public ValueTask AwcAelottoAsync(
        AwcAelottoViewModel model)
    {
        var request = new BrandAwcAelottoMessageRequest
        {
            GameProviderCode = GameProviderType.AWC_AELOTTO,
            BrandCode = model.BrandCode
        };
        return _messageSender
            .PublishAsync(
                subject: "game.playerbalance.brand.awc.aelotto",
                data: request.ToByteArray(),
                cancellationToken: HttpContext.RequestAborted);
    }

    [HttpPost("awc-aws", Name = "AwcAws")]
    public ValueTask AwcAwsAsync(
        AwcAwsViewModel model)
    {
        var request = new BrandAwcAelottoMessageRequest
        {
            GameProviderCode = GameProviderType.AWC_AWS,
            BrandCode = model.BrandCode
        };
        return _messageSender
            .PublishAsync(
                subject: "game.playerbalance.brand.awc.aws",
                data: request.ToByteArray(),
                cancellationToken: HttpContext.RequestAborted);
    }

    [HttpPost(Name = "brand")]
    public ValueTask BrandAsync(
        BrandViewModel model)
    {
        var request = new BrandMessageRequest
        {
            GameProviderCode = model.GameProviderCode,
            BrandCode = model.BrandCode
        };
        return _messageSender
            .PublishAsync(
                subject: "game.playerbalance.brand",
                data: request.ToByteArray(),
                cancellationToken: HttpContext.RequestAborted);
    }
}
