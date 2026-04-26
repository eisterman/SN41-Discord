namespace Main_Bot.Features.GoodByePhrases;

public interface IGoodByePhrasesProvider {
    public string GetRandomPhrase(string username);
}