using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class FundTraceFormQueuedMessageHandler(
	ILogger<FundTraceFormQueuedMessageHandler> logger,
	ICommandService<FundTraceFormQueuedCommand> service) : IMessageHandler<FundTraceFormModelRequest>
{
	public ValueTask HandleAsync(string subject, FundTraceFormModelRequest data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new FundTraceFormQueuedCommand(
				Id: data.FormId),
			cancellationToken: cancellationToken);
	}
}
