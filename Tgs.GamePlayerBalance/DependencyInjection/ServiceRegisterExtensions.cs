using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GamePlayerBalance.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegisterExtensions
{
    public static async Task<WebApplication> UseRegister(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var client = scope.ServiceProvider.GetRequiredService<IGameCommandClient>();

            try
            {
                var gameProviderInfos = await client.GameProviderQueryAsync()
                    .ToArrayAsync()
                    .ConfigureAwait(false)
                    ?? Enumerable.Empty<GameProviderInfo>();

                if (gameProviderInfos.Any())
                {
                    var gameProviderRepository = scope.ServiceProvider.GetRequiredService<IGameProviderRepository>();

                    foreach (var gameProviderInfo in gameProviderInfos)
                    {
                        _ = await gameProviderRepository
                            .GetIdAsync(
                                gameProviderInfo.GameProviderCode)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch
            {
            }

            var brandFormRepository = scope.ServiceProvider.GetRequiredService<IBrandFormRepository>();
            await brandFormRepository
                 .InitializeAsync()
                 .ConfigureAwait(false);

            var importFormRepository = scope.ServiceProvider.GetRequiredService<IImportFormRepository>();
            await importFormRepository
                 .InitializeAsync()
                 .ConfigureAwait(false);

            var playerFormRepository = scope.ServiceProvider.GetRequiredService<IPlayerFormRepository>();
            await playerFormRepository
                 .InitializeAsync()
                 .ConfigureAwait(false);
        }
        return app;
    }
}
