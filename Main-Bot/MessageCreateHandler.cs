using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace Main_Bot;

[UsedImplicitly]
public class MessageCreateHandler(IOptions<SN41Settings> settings, ILogger<MessageCreateHandler> logger)
    : IMessageCreateGatewayHandler {
    public ValueTask HandleAsync(Message message) {
        logger.LogInformation("{}", message.Content);
        logger.LogInformation("Setted: {}", string.Format(settings.Value.WelcomeMessage.Body, message.Author.Username));
        return default;
    }
}