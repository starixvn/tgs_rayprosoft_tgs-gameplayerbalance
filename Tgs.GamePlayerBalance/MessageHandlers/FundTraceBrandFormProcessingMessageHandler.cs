using Tgs.GamePlayerBalance.Common.Protos;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GamePlayerBalance.MessageHandlers;

public class FundTraceBrandFormProcessingMessageHandler(
	ILogger<FundTraceBrandFormProcessingMessageHandler> logger,
	ICommandService<FundTraceBrandFormProcessingCommand> service) : IMessageHandler<FundTraceBrandFormModelRequest>
{
	public ValueTask HandleAsync(string subject, FundTraceBrandFormModelRequest data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Mesaage:This message has been received.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return service.ExecuteAsync(
			command: new FundTraceBrandFormProcessingCommand(
				Id: data.FormId),
			cancellationToken: cancellationToken);
	}
}
