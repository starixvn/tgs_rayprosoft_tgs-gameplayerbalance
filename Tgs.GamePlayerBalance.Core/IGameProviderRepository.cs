using Tgs.GameCommand.Common;
using Tgs.GameProviders.Domains.ConfigurationData;
using Tgs.GameProviders.Domains.SettingConfiguration;

namespace Tgs.GamePlayerBalance.Core;

public interface IGameProviderRepository
{
    ValueTask<Guid> GetIdAsync(
        string code,
        CancellationToken cancellationToken = default);
}
