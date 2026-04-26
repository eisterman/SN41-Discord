using JetBrains.Annotations;
using Main_Bot.Features.GoodByePhrases;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Main_Bot.Gateways;

[UsedImplicitly]
public sealed class OnMemberGuildRemove(
    IOptions<AdminFeatureSettings> settings,
    ILogger<OnMemberGuildRemove> logger,
    IGoodByePhrasesProvider goodByePhrasesAssemblyProvider,
    RestClient restClient
) : IGuildUserRemoveGatewayHandler {
    public async ValueTask HandleAsync(GuildUserRemoveEventArgs arg) {
        var username = arg.User.GlobalName ?? arg.User.Username;
        logger.LogInformation("Member {} leaved the server", username);
        var message = new MessageProperties();
        message.WithContent(goodByePhrasesAssemblyProvider.GetRandomPhrase(username));
        await restClient.SendMessageAsync(settings.Value.AdminLogTextChannelId, message);
    }
}