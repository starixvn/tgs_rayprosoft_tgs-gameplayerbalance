namespace Tgs.GamePlayerBalance.Core.ApplicationServices;

public record ImportFormProcessingPublishCommand(
    long ExpansionFormId,
    long FilterId);
