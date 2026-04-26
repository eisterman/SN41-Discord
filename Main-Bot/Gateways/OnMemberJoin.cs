using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Main_Bot.Gateways;

[UsedImplicitly]
public sealed class OnMemberJoin(IOptions<SN41Settings> settings) : IGuildUserAddGatewayHandler {
    // Here I chose to read from the assembly every time, but it's not the most scalable
    // solution. The best solution would be moving the resource in a singleton.
    // Singletons are simple but lazy, while IHostedServices are more complex but not lazy.
    private static Stream GetImageStream() {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream("Main_Bot.welcome.png") ??
               throw new InvalidOperationException("Missing welcome.png inside assembly resources");
    }

    public async ValueTask HandleAsync(GuildUser guildUser) {
        await using var file = GetImageStream();
        var image = new AttachmentProperties("image.png", file);
        var body = string.Format(settings.Value.WelcomeMsgFeature.Body, guildUser.Username);
        var embed = new EmbedProperties()
            .WithDescription(body)
            .WithColor(new(0xF1C40F))
            .WithImage("attachment://image.png");
        var message = new MessageProperties()
            .WithAttachments([image])
            .WithEmbeds([embed]);
        DMChannel channel = await guildUser.GetDMChannelAsync();
        await channel.SendMessageAsync(message);
    }
}