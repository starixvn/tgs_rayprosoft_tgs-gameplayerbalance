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

		_ = fakeClient
			.GameProviderGetGameProviderIdAsync(
				Arg.Is(code),
				Arg.Any<CancellationToken>())
			.Returns(Guid.NewGuid());

		_ = await repository
			.GetIdAsync(
				code)
			.ConfigureAwait(false);
	}
}
