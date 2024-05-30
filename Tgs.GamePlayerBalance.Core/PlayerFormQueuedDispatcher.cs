using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Tgs.GamePlayerBalance.Core.ApplicationServices;

namespace Tgs.GamePlayerBalance.Core;

internal class PlayerFormQueuedDispatcher(IServiceProvider serviceProvider) : IPlayerFormQueuedDispatcher, IDisposable, IAsyncDisposable
{
	private readonly ConcurrentDictionary<string, PlayerFormQueuedChannel> _counters = new();
	private readonly CancellationTokenSource _cancellationTokenSource = new();

	private bool _disposedValue;

	~PlayerFormQueuedDispatcher()
	{
		Dispose(false);
	}

	public async ValueTask PushCommandAsync(PlayerFormQueuedCommand command, CancellationToken cancellationToken = default)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);

		var counter = _counters.GetOrAdd(
			nameof(PlayerFormQueuedDispatcher),
			key =>
			{
				var scope = serviceProvider.CreateScope();
				var newCounter = ActivatorUtilities.CreateInstance<PlayerFormQueuedChannel>(
					scope.ServiceProvider,
					key);

				newCounter.Disposed += (_, _) =>
				{
					_ = _counters.TryRemove(newCounter.Key, out _);
					scope.Dispose();
				};

				return newCounter;
			});

		await counter.PrepareAsync(cts.Token).ConfigureAwait(false);
		await counter.PushCommandAsync(command, cts.Token).ConfigureAwait(false);
	}

	public ValueTask DisposeAsync()
	{
		Dispose(true);
		GC.SuppressFinalize(this);

		return ValueTask.CompletedTask;
	}

	public void Dispose()
	{
		// 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_cancellationTokenSource.Cancel();
			}

			_cancellationTokenSource.Dispose();
			_disposedValue = true;
		}
	}
}
