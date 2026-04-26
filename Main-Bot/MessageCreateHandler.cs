using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using JetBrains.Annotations;
using Main_Bot.Features;

namespace Main_Bot;

[UsedImplicitly]
public sealed class MessageCreateHandler(
    AntiSpamLogicSingleton antiSpam
) : IMessageCreateGatewayHandler {
    public async ValueTask HandleAsync(Message message) {
        await antiSpam.ProcessMessage(message);
    }
}