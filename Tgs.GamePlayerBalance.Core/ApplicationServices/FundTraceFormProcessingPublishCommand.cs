namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record class FundTraceFormProcessingPublishCommand(
	long ExpansionFormId,
	long FilterId);
