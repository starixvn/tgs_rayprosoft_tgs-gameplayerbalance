using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GamePlayerBalance.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegisterExtensions
{
	public static async Task<WebApplication> InitializeBrandFormAsync(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var brandFormRepository = scope.ServiceProvider.GetRequiredService<IBrandFormRepository>();
			await brandFormRepository
				 .InitializeAsync()
				 .ConfigureAwait(false);
		}

		return app;
	}

	public static async Task<WebApplication> InitializePlayerFormAsync(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var playerFormRepository = scope.ServiceProvider.GetRequiredService<IPlayerFormRepository>();
			await playerFormRepository
				 .InitializeAsync()
				 .ConfigureAwait(false);
		}

		return app;
	}

	public static async Task<WebApplication> InitializePlayerTempBalanceAsync(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var playerTempBalanceRepository = scope.ServiceProvider.GetRequiredService<IPlayerTempBalanceRepository>();
			await playerTempBalanceRepository
				 .InitializeAsync()
				 .ConfigureAwait(false);
		}

		return app;
	}

	public static async Task<WebApplication> InitializeUseImportFormAsync(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var importFormRepository = scope.ServiceProvider.GetRequiredService<IImportFormRepository>();
			await importFormRepository
				 .InitializeAsync()
				 .ConfigureAwait(false);
		}

		return app;
	}

	public static WebApplication InitialSnowflakeWork(this WebApplication app)
	{
		using (var scope = app.Services.CreateScope())
		{
			var snowflakeWork = scope.ServiceProvider.GetRequiredService<ISnowflakeWork>();

			_ = snowflakeWork.GetId();
		}

		return app;
	}

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
						var gameProviderId = await gameProviderRepository.GetIdAsync(
							gameProviderInfo.GameProviderCode)
							.ConfigureAwait(false);

						_ = await gameProviderRepository.GetAsync(
							id: gameProviderId)
							.ConfigureAwait(false);
					}
				}
			}
			catch
			{
			}
		}

		return app;
	}
}
