using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Main_Bot.CommandModules;

internal record ReplayResponse(
    string Status,
    string Message,
    [property: JsonPropertyName("video_url")]
    string VideoUrl,
    [property: JsonPropertyName("builds_url")]
    string BuildsUrl,
    [property: JsonPropertyName("replay_url")]
    string ReplayUrl,
    ReplayMetadata Metadata
);

// ReSharper disable once ClassNeverInstantiated.Global
[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
internal record ReplayMetadata(
    string Filename,
    [property: JsonPropertyName("client_version")]
    string ClientVersion,
    [property: JsonPropertyName("unique_id")]
    string UniqueId
);

[UsedImplicitly]
public class ReplayModule(
    IOptions<RenderReplayFeatureSettings> settings,
    ILogger<ReplayModule> logger,
    IHttpClientFactory httpClientFactory
) : ApplicationCommandModule<ApplicationCommandContext> {
    [UsedImplicitly]
    [SlashCommand("replay", "Crea un nuovo replay")]
    public async Task Replay(
        [SlashCommandParameter(Description = "File .wowsreplay da renderizzare")]
        Attachment attachment
    ) {
        try {
            HttpClient httpClient = httpClientFactory.CreateClient();
            // Verify filename
            if (!attachment.FileName.EndsWith(".wowsreplay")) {
                await RespondAsync(InteractionCallback.Message(new() {
                    Content = "Replay hanno .wowsreplay come estensione",
                    Flags = MessageFlags.Ephemeral
                }));
                return;
            }
            await RespondAsync(InteractionCallback.DeferredMessage());
            // Per ottenere il file raw devi fetcharlo manualmente dall'Url.
            // Url puo scadere, mentre ProxyUrl no. Ma in questo caso non c'e problema
            // essendo l'attachment creato poco tempo fa.
            Stream stream = await httpClient.GetStreamAsync(attachment.Url);
            logger.LogInformation("Starting replay rendering: {filename}", attachment.FileName);
            // Send replay to API via FormData
            var form = new MultipartFormDataContent();
            form.Add(new StreamContent(stream), "file", attachment.FileName);
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", settings.Value.ApiKey);
            await ModifyResponseAsync(msg =>
                msg.WithContent("Render in corso... (puo metterci anche un paio di minuti)"));
            using var apiResponse =
                await httpClient.SendAsync(new(HttpMethod.Post, $"{settings.Value.BaseApiUrl}/api/render"));
            var apiResult = await apiResponse.Content.ReadFromJsonAsync<ReplayResponse>();
            if (apiResult == null) {
                logger.LogWarning("Failed to parse reply for {filename} replay.", attachment.FileName);
                await ModifyResponseAsync(msg => msg.WithContent("Error from API! Call Fede"));
                return;
            }
            // Use API reply to fetch back the video file
            logger.LogInformation("Received API response for {filename}, retrieving video...", attachment.FileName);
            await ModifyResponseAsync(msg => msg.WithContent("Render Completo! Sto caricando il video..."));
            var videoUrl = $"{settings.Value.BaseApiUrl}{apiResult.VideoUrl}";
            var videoFileName = videoUrl.Split('/').Last();
            var videoStream = await httpClient.GetStreamAsync(videoUrl);
            // Send the video file back to Discord
            await ModifyResponseAsync(msg => {
                msg.WithEmbeds([new EmbedProperties().WithDescription($"Input file: {attachment.FileName}")])
                    .WithAttachments([new(videoFileName, videoStream)]);
            });
        } catch (Exception ex) {
            logger.LogError(ex, "Failed to process replay rendering for file: {filename}", attachment.FileName);
            await FollowupAsync(
                new InteractionMessageProperties().WithContent("ERRORE! Urla a Fede di tornare in miniera")
            );
        }
    }
}