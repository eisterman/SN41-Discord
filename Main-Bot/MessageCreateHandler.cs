using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using JetBrains.Annotations;

namespace Main_Bot;

[UsedImplicitly]
public class MessageCreateHandler(ILogger<MessageCreateHandler> logger) : IMessageCreateGatewayHandler {
    public ValueTask HandleAsync(Message message) {
        logger.LogInformation("{}", message.Content);
        return default;
    }
}