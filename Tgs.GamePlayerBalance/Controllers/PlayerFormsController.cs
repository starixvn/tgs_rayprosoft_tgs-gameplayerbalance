using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerFormsController : ControllerBase
{
	[HttpPost(Name = "PlayerFormAdd")]
	public ValueTask PlayerFormAddAsync(
		[FromServices] ICommandService<PlayerFormAddCommand> service,
		PlayerFormAddViewModel source)
	=> service.ExecuteAsync(
		command: new PlayerFormAddCommand(
			GameProviderCode: source.GameProviderCode,
			PlayerCode: source.PlayerCode),
		cancellationToken: HttpContext.RequestAborted);
}
