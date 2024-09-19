using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandTimeFormsController : ControllerBase
{
	[HttpPost(Name = "BrandTimeFormAdd")]
	public ValueTask BrandTimeFormAddAsync(
		[FromServices] ICommandService<BrandTimeFormAddCommand> service,
		BrandTimeFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new BrandTimeFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				BrandCode: source.BrandCode,
				StartTime: source.StartTime,
				EndTime: source.EndTime),
			cancellationToken: HttpContext.RequestAborted);
}
