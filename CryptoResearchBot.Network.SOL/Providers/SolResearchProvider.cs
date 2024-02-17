using CryptoResearchBot.Core.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoResearchBot.Core.Network
{
    public class SolResearchProvider : BaseResearchProvider
    {
        public override string GroupName => throw new NotImplementedException();

        protected override Task HandleMessageInternal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override BaseWatchingTopicData CreateWatchingTopicData(int topicId, ChannelInformation channelInformation, BaseTokenData? tokenData)
        {
            throw new NotImplementedException();
        }

        protected override List<BaseWatchingTopicData> DeserializeTopics(string json)
        {
            throw new NotImplementedException();
        }

        protected override string SerializeTopics()
        {
            throw new NotImplementedException();
        }
    }
}
