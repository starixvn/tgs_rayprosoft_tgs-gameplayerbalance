using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

internal class BrandFormAddCommandService : ICommandService<BrandFormAddCommand>
{
    private readonly IBrandFormRepository _brandFormRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IGameProviderRepository _gameProviderRepository;
    private readonly ILogger<BrandFormAddCommandService> _logger;
    private readonly IMessageSender _messageSender;

    public BrandFormAddCommandService(
        ILogger<BrandFormAddCommandService> logger,
        IBrandRepository brandRepository,
        IGameProviderRepository gameProviderRepository,
        IBrandFormRepository brandFormRepository,
        IMessageSender messageSender
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
        _gameProviderRepository = gameProviderRepository ?? throw new ArgumentNullException(nameof(gameProviderRepository));
        _brandFormRepository = brandFormRepository ?? throw new ArgumentNullException(nameof(brandFormRepository));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask ExecuteAsync(BrandFormAddCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _gameProviderRepository
           .GetIdAsync(
               code: command.GameProviderCode,
               cancellationToken: cancellationToken)
           .ConfigureAwait(false);

        var brandId = await _brandRepository
            .GetIdAsync(
                code: command.BrandCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var id = await _brandFormRepository
            .AddAsync(
                gameProviderId: gameProviderId,
                gameProviderCode: command.GameProviderCode,
                brandId: brandId,
                brandCode: command.BrandCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var request = new BrandFormModelRequest
        {
            Id = id
        };

        var subject = "game.playerbalance.brand.form.queued";
        if (command.GameProviderCode.StartsWith("AWC_", StringComparison.OrdinalIgnoreCase))
        {
            subject = $"{subject}.{command.GameProviderCode.Replace("_", ".").ToLower()}";
        }

        await _messageSender
            .PublishAsync(
                subject: subject,
                data: request.ToByteArray(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("LogOn:{logOn}  Subject:{subject} Information Request:{receive}",
            DateTime.Now.ToString("o"),
            subject,
            request);
    }
}
