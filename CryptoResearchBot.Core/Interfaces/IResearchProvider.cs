using CryptoResearchBot.Core.Data;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoResearchBot.Core.Interfaces
{
    public interface IResearchProvider
    {
        string GroupName { get; }

        void LoadWatchingTopics();
        void SaveWatchingTopics();

        Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}