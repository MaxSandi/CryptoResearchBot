using System.Collections.Concurrent;
using TL;

namespace CryptoResearchBot.Core.TelegramAPI
{
    public static class ChatListener
    {
        private static ConcurrentDictionary<long, Action<long,Message>> _watchingChats = new ConcurrentDictionary<long, Action<long, Message>>();

        public static void Initialize()
        {
            TelegramApiProvider.Client.OnUpdate += Client_OnUpdate;

            Console.WriteLine("Start listening chats...");
        }

        public static bool AddWatchingChat(long chatId, Action<long, Message> callback)
        {
            _watchingChats.AddOrUpdate(chatId, callback, (key, value) => value);
            return true;
        }

        public static bool RemoveWatchingChat(long chatId)
        {            
            return _watchingChats.Remove(chatId, out _);
        }

        private static async Task Client_OnUpdate(TL.UpdatesBase updates)
        {
            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case TL.UpdateNewMessage unm:
                        await HandleMessage(unm.message); break;
                    default: break; // there are much more update types than the above example cases
                }
        }

        private static Task HandleMessage(TL.MessageBase messageBase)
        {
            switch (messageBase)
            {
                case TL.Message m:
                    if (_watchingChats.ContainsKey(m.peer_id.ID))
                        _watchingChats[m.peer_id.ID].Invoke(m.peer_id.ID, m);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
