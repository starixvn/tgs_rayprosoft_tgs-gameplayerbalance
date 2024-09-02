using System.Reflection;
using Tgs.GamePlayerBalance.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGamePlayerBalanceCore(
		this IServiceCollection services)
		=> services
		.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
		.AddSingleton<IPlayerFormQueuedDispatcher, PlayerFormQueuedDispatcher>()
		.AddSingleton<ISnowflakeWork, SnowflakeWork>();
}
