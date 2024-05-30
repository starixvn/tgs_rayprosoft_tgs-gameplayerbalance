using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class PlayerFormQueuedMessageHandler(
	ILogger<PlayerFormQueuedMessageHandler> logger,
	ICommandService<PlayerFormQueuedCommand> service) : IMessageHandler<PlayerFormModelRequest>
{
	public ValueTask HandleAsync(string subject, PlayerFormModelRequest data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new PlayerFormQueuedCommand(
				Id: data.Id),
			cancellationToken: cancellationToken);
	}
}
