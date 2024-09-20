using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class FundTraceGameProviderFormQueuedMessageHandler(
	ILogger<FundTraceGameProviderFormQueuedMessageHandler> logger,
	ICommandService<FundTraceGameProviderFormQueuedCommand> service) : IMessageHandler<FundTraceGameProviderFormModelRequest>
{
	public ValueTask HandleAsync(string subject, FundTraceGameProviderFormModelRequest data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new FundTraceGameProviderFormQueuedCommand(
				Id: data.FormId),
			cancellationToken: cancellationToken);
	}
}
