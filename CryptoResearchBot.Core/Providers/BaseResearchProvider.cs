using CryptoResearchBot.Core.Common;
using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Extensions;
using CryptoResearchBot.Core.Helpers;
using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Parser;
using CryptoResearchBot.Core.Providers;
using CryptoResearchBot.Core.TelegramAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoResearchBot.Core.Network
{
    public abstract class BaseResearchProvider : IResearchProvider
    {
        private readonly string tokensFilePath = Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))) ?? AppDomain.CurrentDomain.BaseDirectory;

        protected ConcurrentDictionary<int, WatchingTopic> _watchingTopics = new();

        protected abstract long ChatId { get; }
        protected abstract string TokensFileName { get; }

        public abstract string GroupName { get; }
        public abstract IFindNewTokenProvider FindNewTokenProvider { get; }
        public abstract ITokenProvider TokenProvider { get; }

        public void LoadWatchingTopics()
        {
            _watchingTopics.Clear();

            var tokenFile = GetTokenFilePath();
            if (System.IO.File.Exists(tokenFile))
            {
                string json = System.IO.File.ReadAllText(tokenFile);
                var topicDatas = DeserializeTopics(json);
                foreach (var item in topicDatas)
                {
                    var watchingTopic = new WatchingTopic(item, TokenProvider, ChatId);
                    watchingTopic.StartListen();

                    _watchingTopics.AddOrUpdate(watchingTopic.Id, watchingTopic, (key, value) => value);
                }
            }
        }

        public void SaveWatchingTopics()
        {
            // сохраняем наблюдаемые токены
            string json = SerializeTopics();

            var tokenFile = GetTokenFilePath();
            System.IO.File.WriteAllText(tokenFile, json);
        }

        public async Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Type switch
            {
                Telegram.Bot.Types.Enums.UpdateType.Message => update.Message,
                Telegram.Bot.Types.Enums.UpdateType.CallbackQuery => update.CallbackQuery is not null ? update.CallbackQuery.Message : null,
                _ => null,
            };
            if (message is not null &&
                message.MessageThreadId is not null)
            {
                if (_watchingTopics.TryGetValue(message.MessageThreadId.Value, out var watchingTopic))
                {
                    await watchingTopic.HandleMessage(botClient, update, cancellationToken);
                    if (update.CallbackQuery is not null &&
                        update.CallbackQuery.Data == TelegramCallbackData.RemoveToken)
                    {
                        watchingTopic.StopListen();
                        _watchingTopics.Remove(message.MessageThreadId.Value, out var _);
                    }
                }
                else
                    await HandleMessageInternal(botClient, update, cancellationToken);
            }
        }

        protected async Task<WatchingTopic?> CreateWatchingTopicAsync(ITelegramBotClient botClient, string groupLink, BaseTokenData? tokenData, CancellationToken cancellationToken)
        {
            var channelInformation = await TelegramApiProvider.JoinToChat(groupLink);
            if (channelInformation is null)
                return null;

            // добавляем новый топик
            var topic = await botClient.CreateForumTopicAsync(ChatId, channelInformation.Name);

            // добавляем сообщение со статистикой канала (основное)
            var topicData = CreateWatchingTopicData(topic.MessageThreadId, channelInformation, tokenData);
            var message = await botClient.SendTokenMessage(ChatId, topicData.GetMainInformation(), topic.MessageThreadId, cancellationToken);
            topicData.MainMessageId = message.MessageId;

            // добавляем сообщение с информацией о коллах (если есть)
            await CallTokenHelper.PrepareTotalCallMessage(topicData);

            var newWatchingTopic = new WatchingTopic(topicData, TokenProvider, ChatId);
            // начинаем следить за сообщениями канала
            newWatchingTopic.StartListen();

            return newWatchingTopic;
        }

        public abstract Task HandleNewTokens(ITelegramBotClient botClient, IEnumerable<ITokenData> tokens);

        protected abstract List<BaseWatchingTopicData> DeserializeTopics(string json);
        protected abstract string SerializeTopics();

        protected abstract Task HandleMessageInternal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);

        protected abstract BaseWatchingTopicData CreateWatchingTopicData(int topicId, ChannelInformation channelInformation, BaseTokenData? tokenData);

        #region Private methods
        private string GetTokenFilePath()
        {
            return Path.Combine(tokensFilePath, TokensFileName);
        }
        #endregion
    }
}
