using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class BrandTimeFormProcessingMessageHandler(
	ILogger<BrandTimeFormProcessingMessageHandler> logger,
	ICommandService<BrandTimeFormProcessingCommand> service) : IMessageHandler<BrandTimeFormModelRequest>
{
	public ValueTask HandleAsync(string subject, BrandTimeFormModelRequest data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new BrandTimeFormProcessingCommand(
				Id: data.FormId),
			cancellationToken: cancellationToken);
	}
}
