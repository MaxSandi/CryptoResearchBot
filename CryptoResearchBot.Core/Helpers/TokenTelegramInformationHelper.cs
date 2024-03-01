using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.TelegramAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace CryptoResearchBot.Core.Helpers
{
    public static class TokenTelegramInformationHelper
    {
        private static ConcurrentDictionary<string, Action<long, Message>> _watchingTokens = new ConcurrentDictionary<string, Action<long, Message>>();

        public static void Initialize()
        {
            var channel = TelegramApiProvider.Chats.Values.SingleOrDefault(x => x.Title.Contains("Early Calls BOT"));
            if (channel is not null)
            {
                ChatListener.AddWatchingChat(channel.ID, (long id, Message message) =>
                {
                    foreach (var watchingToken in _watchingTokens)
                    {
                        if (message.message.Contains(watchingToken.Key))
                            watchingToken.Value.Invoke(id, message);
                    }
                });
            }
        }

        public static bool AddWatchingToken(string tokenId, Action<long, Message> callback)
        {
            _watchingTokens.AddOrUpdate(tokenId, callback, (key, value) => value);
            return true;
        }

        public static bool RemoveWatchingToken(string tokenId)
        {
            return _watchingTokens.Remove(tokenId, out _);
        }

        public static async Task PrepareTotalCallMessage(BaseWatchingTopicData watchingTopicData)
        {
            if (watchingTopicData.Token is null)
                return;

            var channel = TelegramApiProvider.Chats.Values.SingleOrDefault(x => x.Title.Contains("Early Calls BOT"));
            if (channel is not Channel callChannel)
                return;

            var messages = await TelegramApiProvider.Client.Messages_Search(callChannel, watchingTopicData.Token.Id);
            if (messages is null)
                return;

            var totalCallMessage = messages.Messages.OfType<TL.Message>().FirstOrDefault(x =>
            {
                string[] groupsArray = x.message.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                return groupsArray.Length > 0 && groupsArray[0].Contains("TOTAL CALLS");
            });
            if(totalCallMessage is not null)
            {
                var message = await TelegramApiProvider.ForwardMessageFromWatchingChat(totalCallMessage, callChannel, watchingTopicData.Id, true);
                watchingTopicData.TotalCallMessageId = message.ID;
            }
        }
    }
}
