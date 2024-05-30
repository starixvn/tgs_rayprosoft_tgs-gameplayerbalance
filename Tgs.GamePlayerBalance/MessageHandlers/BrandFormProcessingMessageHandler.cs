using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class BrandFormProcessingMessageHandler(
	ILogger<BrandFormProcessingMessageHandler> logger,
	ICommandService<BrandFormProcessingCommand> service) : IMessageHandler<BrandFormModelRequest>
{
	public ValueTask HandleAsync(string subject, BrandFormModelRequest data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new BrandFormProcessingCommand(
				Id: data.Id),
			cancellationToken: cancellationToken);
	}
}
