using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Network;
using CryptoResearchBot.Core.Parser;
using CryptoResearchBot.Core.Providers;
using CryptoResearchBot.ETH.Data;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoResearchBot.ETH.Providers
{
    public class EthResearchProvider : BaseResearchProvider
    {
        protected override string TokensFileName => "eth_tokens.json";
        protected override long ChatId => -1002084456610;

        public override string GroupName => "ETHResearch";

        private IFindNewTokenProvider _findNewTokenProvider = new EthFindNewTokenProvider();
        public override IFindNewTokenProvider FindNewTokenProvider => _findNewTokenProvider;

        public override ITokenProvider TokenProvider => throw new NotImplementedException();

        protected override async Task HandleMessageInternal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not null &&
                update.Message.Text is not null &&
                update.Message.MessageThreadId is not null)
            {
                if (update.Message.MessageThreadId == 3) // основа - считываем тг ссылки
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

        public override Task HandleNewTokens(ITelegramBotClient botClient, IEnumerable<ITokenData> tokens)
        {
            return Task.CompletedTask;
        }

        protected override BaseWatchingTopicData CreateWatchingTopicData(int topicId, ChannelInformation channelInformation, BaseTokenData? tokenData)
        {
            return new EthWatchingTopicData(topicId, channelInformation, tokenData);
        }

        protected override List<BaseWatchingTopicData> DeserializeTopics(string json)
        {
            var topicDatas = JsonConvert.DeserializeObject<List<EthWatchingTopicData>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            if (topicDatas is null)
                return new List<BaseWatchingTopicData>();

            return topicDatas.OfType<BaseWatchingTopicData>().ToList();
        }

        protected override string SerializeTopics()
        {
            List<EthWatchingTopicData> topicDatas = _watchingTopics.Select(x => x.Value.Data).OfType<EthWatchingTopicData>().ToList();
            return JsonConvert.SerializeObject(topicDatas, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}
