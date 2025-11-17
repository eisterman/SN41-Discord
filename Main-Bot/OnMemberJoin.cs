using System.Reflection;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using NetCord;

namespace Main_Bot;

[UsedImplicitly]
public class OnMemberJoin(IOptions<SN41Settings> settings) : IGuildUserAddGatewayHandler {
    private static Stream GetImageStream() {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream("Main_Bot.welcome.png") ??
               throw new InvalidOperationException("Missing welcome.png inside assembly resources");
    }

    public async ValueTask HandleAsync(GuildUser guildUser) {
        await using var file = GetImageStream();
        var image = new AttachmentProperties("image.png", file);
        var body = string.Format(settings.Value.WelcomeMessage.Body, guildUser.Username);
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