using NetCord;
using NetCord.Gateway;

namespace Main_Bot.Extensions;

public static class GuildUserExtensions {
    extension(GuildUser value) {
        public bool HasRoleName(Guild guild, IEnumerable<string> rolenames) {
            return value.GetRoles(guild).Select((role => role.Name)).Any(rolenames.Contains);
        }
    }
}