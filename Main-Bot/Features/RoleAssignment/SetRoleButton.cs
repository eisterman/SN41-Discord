using JetBrains.Annotations;
using Main_Bot.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace Main_Bot.Features.RoleAssignment;

public class SetRoleButton(
    IOptions<SN41Settings> settings,
    ILogger<SetRoleButton> logger,
    RestClient restClient
) : ComponentInteractionModule<ButtonInteractionContext> {
    [UsedImplicitly]
    [ComponentInteraction("setrolebtn")]
    public async Task Button(string label, List<string> roles, GuildUser targetUser) {
        if (Context.Guild == null) return;
        var requestingUser = await Context.Guild.GetUserAsync(Context.User.Id);
        // Check and report if user is trying to change the roles of another admin
        if (!requestingUser.HasRoleName(Context.Guild, settings.Value.AdminRoles)) {
            logger.LogWarning(
                "Roleset - User {requestingUName} ILLEGALLY tried to set the role {label} to {targetUName}",
                Context.User.Username, label, targetUser.Username);
            await Context.Message.DeleteAsync();
            var dmChannel = await Context.User.GetDMChannelAsync();
            await dmChannel.SendMessageAsync(new() {
                Content = "**ATTENZIONE!** E' stato rilevato un tentativo illegale di cambio utente.\n" +
                          "Tale azione e' stata reportata agli amministratori."
            });
            await restClient.SendMessageAsync(settings.Value.AdminFeature.AdminLogTextChannelId, new() {
                Content = $"**ATTENZIONE!** UTENTE {requestingUser.Username} HA TENTATO DI CAMBIARE I RUOLI " +
                          $"(set to {label}) ALL'UTENTE {targetUser.Username} IN MANIERA ILLEGALE!"
            });
            return;
        }
        var replyChannelId = Context.Message.ChannelId;
        await Context.Message.DeleteAsync();
        logger.LogInformation("Roleset - User {requestingUName} set role {label} to {targetUName}",
            Context.User.Username, label, targetUser.Username);
        var roleIdsToSet = (await Context.Guild.GetRolesAsync()).Where(r => roles.Contains(r.Name)).Select(r => r.Id);
        await targetUser.ModifyAsync(options => { options.RoleIds = roleIdsToSet; });
        await restClient.SendMessageAsync(replyChannelId, new() {
            Content = $"L'utente {targetUser.Username} e' stato assegnato da {Context.User.Username} al gruppo {label}"
        });
    }
}