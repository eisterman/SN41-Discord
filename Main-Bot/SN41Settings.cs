using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
// ReSharper disable ClassNeverInstantiated.Global

namespace Main_Bot;

public record SN41Settings {
    [Required] [ValidateObjectMembers] public required WelcomeMsgSettings WelcomeMessage { get; init; }
}

public record WelcomeMsgSettings {
    [Required] public required string Body { get; init; }
}