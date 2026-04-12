using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Main_Bot;

[UsedImplicitly]
public sealed class OnMemberRemove(
    IOptions<GoodByeFeatureSettings> settings,
    ILogger<MessageCreateHandler> logger,
    GoodByePhrasesProvider goodByePhrasesProvider,
    RestClient restClient
) : IGuildUserRemoveGatewayHandler {
    public async ValueTask HandleAsync(GuildUserRemoveEventArgs arg) {
        var username = arg.User.GlobalName ?? arg.User.Username;
        logger.LogInformation("Member {} leaved the server", username);
        var message = new MessageProperties();
        message.WithContent(goodByePhrasesProvider.GetRandomPhrase(username));
        await restClient.SendMessageAsync(settings.Value.TextChannelId, message);
    }
}