using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoResearchBot.Core.Providers
{
    public interface IResearchProvider
    {
        IFindNewTokenProvider FindNewTokenProvider { get; }
        ITokenProvider TokenProvider { get; }

        string GroupName { get; }

        void LoadWatchingTopics();
        void SaveWatchingTopics();

        Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
        Task HandleNewTokens(ITelegramBotClient botClient, IEnumerable<ITokenData> tokens);
    }
}