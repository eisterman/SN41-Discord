using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace Main_Bot.Features;

public enum ProcessMessageResult {
    Accepted,
    Rejected,
}

public class MsgEntry {
    public required string Hashmsg { get; set; }
    public required DateTime Timestamp { get; set; }
    public required int Quantity { get; set; }
    public required List<Message> Messages { get; set; }
}

public class AntiSpamLogicSingleton(
    IOptions<SN41Settings> settings,
    ILogger<AntiSpamLogicSingleton> logger,
    RestClient restClient
) {
    private readonly Dictionary<ulong, MsgEntry> _msgEntries = new();

    public async ValueTask<ProcessMessageResult> ProcessMessage(Message msg) {
        // TODO: Split in different rules customizable at runtime?
        // Accept Bot and outside-guild messages
        if (msg.Author.IsBot || msg.Guild == null) return ProcessMessageResult.Accepted;

        // Accept admin messages
        var guildUser = await msg.Guild.GetUserAsync(msg.Author.Id);
        var guildUserRoles = guildUser.GetRoles(msg.Guild).Select((role => role.Name));
        if (guildUserRoles.Any(settings.Value.AdminRoles.Contains)) return ProcessMessageResult.Accepted;

        // Accept messages shorter than 10 chars
        if (msg.Content.Length < 10) return ProcessMessageResult.Accepted;

        // Apply AntiSpam logic
        var now = DateTime.UtcNow;
        var hash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(msg.Content)));
        if (_msgEntries.TryGetValue(msg.Author.Id, out MsgEntry? entry)) {
            // Accept message is too new compared to previous one
            //  or hashes are different
            if (now - entry.Timestamp > TimeSpan.FromMinutes(1) || hash != entry.Hashmsg) {
                entry.Hashmsg = hash;
                entry.Timestamp = now;
                entry.Quantity = 1;
                entry.Messages = [msg];
                return ProcessMessageResult.Accepted;
            }

            // Hashes are the same, register the event
            entry.Quantity += 1;
            entry.Messages.Add(msg);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (entry.Quantity >= 2) {
                // Log (different if under or over the limit)
                if (entry.Quantity >= 3) {
                    logger.LogWarning("Spam - Member {Username} timeouted (n={N})", msg.Author.Username,
                        entry.Quantity);
                } else {
                    logger.LogWarning("Spam - Member {Username} triggered antispam filter (n={N})",
                        msg.Author.Username,
                        entry.Quantity
                    );
                }
                // Remove messages and notify the user
                await Task.WhenAll(entry.Messages.Select(message => message.DeleteAsync()));
                var dmChannel = await msg.Author.GetDMChannelAsync();
                var deleteMessage = new MessageProperties();
                deleteMessage.WithContent(settings.Value.AntiSpamFeature.DeleteMessageContent);
                await dmChannel.SendMessageAsync(deleteMessage);
                entry.Messages.Clear();
                // Over the limit, apply timeout and notify the admins
                if (entry.Quantity >= 3) {
                    await guildUser.TimeOutAsync(now + TimeSpan.FromMinutes(5));
                    var notifMsg = new MessageProperties {
                        Content = $"User {msg.Author.Username} timeouted for repeated msg:\n{msg.Content}"
                    };
                    await restClient.SendMessageAsync(settings.Value.AdminFeature.AdminLogTextChannelId, notifMsg);
                }
                return ProcessMessageResult.Rejected;
            }
        } else {
            // No previous entry for this user
            _msgEntries.Add(msg.Author.Id, new MsgEntry {
                Hashmsg = hash,
                Timestamp = now,
                Quantity = 1,
                Messages = [msg]
            });
        }
        return ProcessMessageResult.Accepted;
    }
}