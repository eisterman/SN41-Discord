using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

// ReSharper disable ClassNeverInstantiated.Global

namespace Main_Bot;

public record SN41Settings {
    [Required]
    [ValidateObjectMembers]
    public required GoodByePhrasesFeatureSettings GoodByePhrasesFeature { get; init; }

    [Required] [ValidateObjectMembers] public required WelcomeMsgFeatureSettings WelcomeMsgFeature { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required DuplicateVoiceChannelFeatureSettings DuplicateVoiceChannelFeature { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required RoleAssignmentFeatureSettings RoleAssignmentFeature { get; init; }

    [Required] [ValidateObjectMembers] public required RenderReplayFeatureSettings RenderReplayFeature { get; init; }
    [Required] [ValidateObjectMembers] public required AssignRolesFeatureSettings AssignRolesFeature { get; init; }
    [Required] [ValidateObjectMembers] public required AdminFeatureSettings AdminFeature { get; init; }
    [Required] [ValidateObjectMembers] public required AntiSpamFeatureSettings AntiSpamFeature { get; init; }
    [Required] public required List<string> AdminRoles { get; init; }
}

public record GoodByePhrasesFeatureSettings {
    [Required] public required ulong GoodByeChannelId { get; init; }
}

public record RoleAssignmentFeatureSettings {
    [Required] public required string CommunityRoleName { get; init; }
    [Required] public required string GuestsRoleName { get; init; }
    [Required] public required List<string> ClanRoleNames { get; init; }
}

public record RenderReplayFeatureSettings {
    [Required] public required string ApiKey { get; init; }
    [Required] public required string BaseApiUrl { get; init; }
}

public record WelcomeMsgFeatureSettings {
    [Required] public required string Body { get; init; }
}

public record DuplicateVoiceChannelFeatureSettings {
    [Required] public required ulong OriginalChannelId { get; init; }
}

public record AssignRolesFeatureSettings {
    [Required] public required ulong TextChannelId { get; init; }
}

public record AdminFeatureSettings {
    [Required] public required ulong AdminLogTextChannelId { get; init; }
}

public record AntiSpamFeatureSettings {
    [Required] public required string DeleteMessageContent { get; init; }
}