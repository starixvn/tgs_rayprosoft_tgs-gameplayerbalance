namespace Tgs.GamePlayerBalance.Core;

public interface IBrandRepository
{
	ValueTask<Models.BrandInfo> GetAsync(string code, CancellationToken cancellationToken = default);
}
