using CryptoResearchBot.Core.Common;
using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Extensions;
using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Parser;
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
        private readonly string tokensFileName = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))) ?? AppDomain.CurrentDomain.BaseDirectory, "tokens.json");

        protected ConcurrentDictionary<int, WatchingTopic> _watchingTopics = new();

        public abstract string GroupName { get; }

        public void LoadWatchingTopics()
        {
            _watchingTopics.Clear();

            if (System.IO.File.Exists(tokensFileName))
            {
                string json = System.IO.File.ReadAllText(tokensFileName);
                var topicDatas = DeserializeTopics(json);
                foreach (var item in topicDatas)
                {
                    var watchingTopic = new WatchingTopic(item);
                    watchingTopic.StartListen();

                    _watchingTopics.AddOrUpdate(watchingTopic.Id, watchingTopic, (key, value) => value);
                }
            }
        }

        public void SaveWatchingTopics()
        {
            // сохраняем наблюдаемые токены
            string json = SerializeTopics();
            System.IO.File.WriteAllText(tokensFileName, json);
        }

        public async Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not null &&
                update.Message.Text is not null &&
                update.Message.MessageThreadId is not null)
            {
                if (_watchingTopics.TryGetValue(update.Message.MessageThreadId.Value, out var watchingTopic))
                    await watchingTopic.HandleMessage(botClient, update, cancellationToken);
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
            var topic = await botClient.CreateForumTopicAsync(TelegramConstants.ChatId, channelInformation.Name);

            // добавляем основное сообщение
            var topicData = CreateWatchingTopicData(topic.MessageThreadId, channelInformation, tokenData);
            var message = await botClient.SendTokenMessage(topicData.GetMainInformation(), topic.MessageThreadId, cancellationToken);
            topicData.MainMessageId = message.MessageId;

            var newWatchingTopic = new WatchingTopic(topicData);
            // начинаем следить за сообщениями канала
            newWatchingTopic.StartListen();
            return newWatchingTopic;
        }


        protected abstract List<BaseWatchingTopicData> DeserializeTopics(string json);
        protected abstract string SerializeTopics();

        protected abstract Task HandleMessageInternal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);

        protected abstract BaseWatchingTopicData CreateWatchingTopicData(int topicId, ChannelInformation channelInformation, BaseTokenData? tokenData);
    }
}
