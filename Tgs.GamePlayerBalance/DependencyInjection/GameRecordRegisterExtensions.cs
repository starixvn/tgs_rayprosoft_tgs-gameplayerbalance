using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GamePlayerBalance.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class GameRecordRegisterExtensions
{
    public static async Task<WebApplication> UseGameRecordRegister(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var client = scope.ServiceProvider.GetRequiredService<IGameCommandClient>();

            var gameProviderIdInfos = await client
                .GameProviderGetGameProviderIdsAsync()
                .ToArrayAsync()
                .ConfigureAwait(false)
                ?? Enumerable.Empty<GameProviderIdInfo>();

            if (gameProviderIdInfos.Any())
            {
                var gameProviderRepository = scope.ServiceProvider.GetRequiredService<IGameProviderRepository>();

                foreach (var gameProviderIdInfo in gameProviderIdInfos)
                {
                    _ = await gameProviderRepository
                        .GetIdAsync(
                            gameProviderIdInfo.GameProviderCode)
                        .ConfigureAwait(false);
                }
            }


        }
        return app;
    }
}
