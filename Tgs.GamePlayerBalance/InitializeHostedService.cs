using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance;

public sealed class InitializeHostedService(
	IBrandFormRepository brandFormRepository,
	IPlayerFormRepository playerFormRepository,
	IGameProviderTimeFormRepository gameProviderTimeFormRepository,
	IBrandTimeFormRepository brandTimeFormRepository,
	IImportFormRepository importFormRepository,
	ISnowflakeWork snowflakeWork) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		var tasks = new Task[]
		{
			Task.Run(()=>brandFormRepository.InitializeAsync(cancellationToken)),
			Task.Run(()=>importFormRepository.InitializeAsync(cancellationToken)),
			Task.Run(()=>brandTimeFormRepository.InitializeAsync(cancellationToken)),
			Task.Run(()=>gameProviderTimeFormRepository.InitializeAsync(cancellationToken)),
			Task.Run(()=>playerFormRepository.InitializeAsync(cancellationToken)),
			Task.Run(()=>snowflakeWork.GetId())
		};

		await Task.WhenAll(tasks).ConfigureAwait(false);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
