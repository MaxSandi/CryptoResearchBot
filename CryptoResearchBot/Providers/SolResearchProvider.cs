using CryptoResearchBot.Core.Common;
using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Network;
using CryptoResearchBot.Core.Parser;
using CryptoResearchBot.Core.Providers;
using CryptoResearchBot.Core.TelegramAPI;
using CryptoResearchBot.SOL.Data;
using CryptoResearchBot.SOL.Extensions;
using CryptoResearchBot.SOL.Providers;
using CryptoResearchBot.SOL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;
using TL;

namespace CryptoResearchBot.SOL
{
    internal class SolResearchProvider : BaseResearchProvider
    {
        public override string GroupName => "SolResearch";

        protected override long ChatId => -1002097195474;
        protected override string TokensFileName => "sol_tokens.json";

        private IFindNewTokenProvider _findNewTokenProvider = new SolFindNewTokenProvider();
        public override IFindNewTokenProvider FindNewTokenProvider => _findNewTokenProvider;

        public override ITokenProvider TokenProvider => new SolTokenProvider();

        #region Handle messages
        public override async Task HandleNewTokens(ITelegramBotClient botClient, IEnumerable<ITokenData> tokens)
        {
            foreach (var token in tokens)
            {
                if (token is SolTokenData solToken)
                {
                    await botClient.NewTokenFindMessage(ChatId, solToken);
                }
            }
        }

        protected override async Task HandleMessageInternal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery && update.CallbackQuery is not null && update.CallbackQuery.Message is not null && update.CallbackQuery.Data is not null)
            {
                switch (update.CallbackQuery.Message.MessageThreadId)
                {
                    case (int)TopicType.NewTokens:
                    case (int)TopicType.SniperTokens:
                        {
                            switch (update.CallbackQuery.Data)
                            {
                                case TelegramCallbackData.ApproveTokenCallbackData:
                                    //await botClient.SendTextMessageAsync(chatId: 6582773624, "Введите ссылку на ТГ", replyMarkup: new ForceReplyMarkup());
                                    // send message 
                                    //await botClient.SendTextMessageAsync(-1002097195474, "test", replyToMessageId: update.CallbackQuery.Message.MessageId, messageThreadId: update.CallbackQuery.Message.MessageThreadId);
                                    //await botClient.ForwardMessageAsync(-1002097195474, -1002097195474, update.CallbackQuery.Message.MessageId, messageThreadId: (int)TopicType.WatchingTokens);
                                    break;
                                case TelegramCallbackData.DeleteTokenCallbackData:
                                    await botClient.DeleteMessageAsync(ChatId, update.CallbackQuery.Message.MessageId);
                                    break;

                                default:
                                    break;
                            }
                            break;
                        }
                    default:
                        break;
                }

            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message is not null && update.Message.ReplyToMessage is not null && update.Message.Text is not null)
            {
                if (update.Message.MessageThreadId == (int)TopicType.NewTokens || update.Message.MessageThreadId == (int)TopicType.SniperTokens)
                {
                    await botClient.DeleteMessageAsync(ChatId, update.Message.MessageId);

                    var tokenData = SolTokenData.CreateTokenData(update.Message.ReplyToMessage);

                    var watchingTopic = await CreateWatchingTopicAsync(botClient, update.Message.Text, tokenData, cancellationToken);
                    if (watchingTopic is not null)
                    {
                        //// пересылаем сообщение в отслеживаемые
                        //await botClient.ForwardMessageAsync(ChatId, ChatId, update.Message.ReplyToMessage.MessageId, (int)TopicType.WatchingTokens);

                        _watchingTopics.AddOrUpdate(watchingTopic.Id, watchingTopic, (key, value) => value);

                        // пометить сообщение как обработанное
                        var newMessage = $"""
                                        {update.Message.ReplyToMessage.Text}

                                        {MarkdownParser.GetConvertedText("🛀🛀🛀🛀🛀🛀🛀🛀🛀🛀🛀🛀🛀🛀🛀🛀")}
                                        """;
                        await botClient.EditMessageTextAsync(
                            chatId: ChatId,
                            messageId: update.Message.ReplyToMessage.MessageId,
                            entities: update.Message.ReplyToMessage.Entities,
                            text: newMessage,
                            disableWebPagePreview: true,
                            cancellationToken: cancellationToken,
                            replyMarkup: TelegramMessageExtensions.NewFindedTokenKeyboard);

                        Console.WriteLine("Join to telegram channel!");
                    }
                    else
                    {
                        // пометить сообщение как необработанное
                        var newMessage = $"""
                                        {update.Message.ReplyToMessage.Text}

                                        {MarkdownParser.GetConvertedText("❗❗❗❗❗❗❗❗❗❗❗❗❗❗❗❗❗❗❗❗❗")}
                                        """;
                        await botClient.EditMessageTextAsync(
                            chatId: ChatId,
                            messageId: update.Message.ReplyToMessage.MessageId,
                            entities: update.Message.ReplyToMessage.Entities,
                            text: newMessage,
                            disableWebPagePreview: true,
                            cancellationToken: cancellationToken,
                            replyMarkup: TelegramMessageExtensions.NewFindedTokenKeyboard);
                    }
                }
                else if (update.Message.MessageThreadId == (int)TopicType.WatchingTokens)
                {

                }
                else if (update.Message.MessageThreadId == (int)TopicType.Base)
                {
                    var watchingTopic = await CreateWatchingTopicAsync(botClient, update.Message.Text, null, cancellationToken);
                    if (watchingTopic is not null)
                    {
                        _watchingTopics.AddOrUpdate(watchingTopic.Id, watchingTopic, (key, value) => value);

                        await botClient.SendTextMessageAsync(
                            chatId: ChatId,
                            replyToMessageId: update.Message.MessageId,
                            text: $"{MarkdownParser.GetConvertedText("✅✅✅✅✅✅")}",
                            cancellationToken: cancellationToken);
                        Console.WriteLine("Join to telegram channel!");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: ChatId,
                            replyToMessageId: update.Message.MessageId,
                            text: $"{MarkdownParser.GetConvertedText("❗❗❗❗❗❗❗")}",
                            cancellationToken: cancellationToken);
                    }
                }
            }
        }

        #endregion

        #region Serialization
        protected override List<BaseWatchingTopicData> DeserializeTopics(string json)
        {
            var topicDatas = JsonConvert.DeserializeObject<List<SolWatchingTopicData>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            if (topicDatas is null)
                return new List<BaseWatchingTopicData>();

            return topicDatas.OfType<BaseWatchingTopicData>().ToList();
        }

        protected override string SerializeTopics()
        {
            List<SolWatchingTopicData> topicDatas = _watchingTopics.Select(x => x.Value.Data).OfType<SolWatchingTopicData>().ToList();
            return JsonConvert.SerializeObject(topicDatas, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
        #endregion

        #region Prepare information
        protected override async Task PrepareTrendingInfoAsync(WatchingTopic watchingTopic)
        {
            if (watchingTopic.Data.Token is null)
                return;

            var baseChannel = TelegramApiProvider.Chats.Values.SingleOrDefault(x => x.Title.Contains("DELUGE.CASH | EVENTS"));
            if (baseChannel is not TL.Channel channel)
                return;

            ChatListener.AddWatchingChat(channel.ID, (long id, TL.Message message) =>
            {
                foreach (var watchingToken in _watchingTokens)
                {
                    if (message.message.Contains(watchingToken.Key))
                        watchingToken.Value.Invoke(id, message);
                }
            });

            var messages = await TelegramApiProvider.Client.Messages_Search(channel, watchingTopic.Data.Token.Id);
            if (messages is null)
                return;

            var message = messages.Messages.OfType<TL.Message>().FirstOrDefault();
            if (message is not null)
                await TelegramApiProvider.ForwardMessageFromWatchingChat(message, channel, watchingTopic.Id, true);

            var channel = TelegramApiProvider.Chats.Values.SingleOrDefault(x => x.Title.Contains("Early Calls BOT"));
            if (channel is not null)
            {

            }
        }
        #endregion

        protected override BaseWatchingTopicData CreateWatchingTopicData(int topicId, ChannelInformation channelInformation, BaseTokenData? tokenData)
        {
            return new SolWatchingTopicData(topicId, channelInformation, tokenData);
        }
    }
}
