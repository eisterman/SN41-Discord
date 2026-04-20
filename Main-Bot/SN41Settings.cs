using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

// ReSharper disable ClassNeverInstantiated.Global

namespace Main_Bot;

public record SN41Settings {
    [Required] [ValidateObjectMembers] public required WelcomeMsgFeatureSettings WelcomeMsgFeature { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required DuplicateVoiceChannelFeatureSettings DuplicateVoiceChannelFeature { get; init; }

    [Required] [ValidateObjectMembers] public required AssignRolesFeatureSettings AssignRolesFeature { get; init; }
    [Required] [ValidateObjectMembers] public required AdminFeatureSettings AdminFeature { get; init; }
    [Required] [ValidateObjectMembers] public required AntiSpamFeatureSettings AntiSpamFeature { get; init; }
    [Required] public required string RenderReplayApiKey { get; init; }
    [Required] public required List<string> AdminRoles { get; init; }
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