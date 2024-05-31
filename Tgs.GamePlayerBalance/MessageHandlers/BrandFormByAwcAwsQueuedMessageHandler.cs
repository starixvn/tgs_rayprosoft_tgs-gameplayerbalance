using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class BrandFormByAwcAwsQueuedMessageHandler(
	ILogger<BrandFormByAwcAwsQueuedMessageHandler> logger,
	ICommandService<BrandFormByAwcAwsQueuedCommand> service) : IMessageHandler<BrandFormModelRequest>
{
	public ValueTask HandleAsync(string subject, BrandFormModelRequest data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new BrandFormByAwcAwsQueuedCommand(
				Id: data.FormId),
			cancellationToken: cancellationToken);
	}
}
