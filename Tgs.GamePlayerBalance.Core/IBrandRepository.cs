namespace Tgs.GamePlayerBalance.Core;

public interface IBrandRepository
{
	ValueTask<Guid> GetIdAsync(
		string code,
		CancellationToken cancellationToken = default);
}
