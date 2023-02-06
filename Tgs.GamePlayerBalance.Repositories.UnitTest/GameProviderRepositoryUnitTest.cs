using NSubstitute;
using Tgs.GameCommand.Client;

namespace Tgs.GamePlayerBalance.Repositories.UnitTest;

[TestFixture]
public class GameProviderRepositoryUnitTest
{
    [Test]
    public async Task Get_Id()
    {
        var fakeClient = Substitute.For<IGameCommandClient>();

        var repository = new GameProviderRepository(fakeClient);

        var code = "KY";

        _ = await repository
            .GetIdAsync(
                code)
            .ConfigureAwait(false);

        _ = repository
            .Received(1)
            .GetIdAsync(
                Arg.Is(code),
                Arg.Any<CancellationToken>());
    }
}
