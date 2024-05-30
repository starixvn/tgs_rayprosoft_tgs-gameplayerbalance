using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandFormsController : ControllerBase
{
	[HttpPost(Name = "BrandFormAdd")]
	public ValueTask BrandFormAddAsync(
		[FromServices] ICommandService<BrandFormAddCommand> service,
		BrandFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new BrandFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				BrandCode: source.BrandCode),
			cancellationToken: HttpContext.RequestAborted);
}
