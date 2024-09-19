using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameProviderTimeFormsController : ControllerBase
{
	[HttpPost(Name = "GameProviderTimeFormAdd")]
	public ValueTask GameProviderTimeFormAddAsync(
		[FromServices] ICommandService<GameProviderTimeFormAddCommand> service,
		GameProviderTimeFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new GameProviderTimeFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				StartTime: source.StartTime,
				EndTime: source.EndTime),
			cancellationToken: HttpContext.RequestAborted);
}
