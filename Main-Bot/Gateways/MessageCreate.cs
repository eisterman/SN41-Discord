using JetBrains.Annotations;
using Main_Bot.Features;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Main_Bot.Gateways;

[UsedImplicitly]
public class MessageCreate(
    AntiSpamLogicSingleton antiSpamLogicSingleton
) : IMessageCreateGatewayHandler {
    public async ValueTask HandleAsync(Message arg) {
        await antiSpamLogicSingleton.ProcessMessage(arg);
    }
}