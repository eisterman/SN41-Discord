using System.Reflection;

namespace Main_Bot;

public sealed class GoodByePhrasesAssemblyProvider : IGoodByePhrasesProvider {
    private static readonly Random Random = new();
    private List<string> Phrases { get; }

    public GoodByePhrasesAssemblyProvider() {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("Main_Bot.goodbye_phrases.txt") ??
                           throw new InvalidOperationException("Missing welcome.png inside assembly resources");
        using var reader = new StreamReader(stream);
        Phrases = reader.ReadToEnd()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    public string GetRandomPhrase(string username) {
        return string.Format(Phrases[Random.Next(Phrases.Count)], username);
    }
}