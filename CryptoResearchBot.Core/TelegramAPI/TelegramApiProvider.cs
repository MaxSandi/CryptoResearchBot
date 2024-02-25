using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Helpers;
using CryptoResearchBot.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace CryptoResearchBot.Core.TelegramAPI
{
    public static class TelegramApiProvider
    {
        public static WTelegram.Client Client { get; } = new WTelegram.Client(Environment.GetEnvironmentVariable);

        public static User Me { get; private set; } = null;

        public static readonly Dictionary<long, TL.User> Users = new();
        public static readonly Dictionary<long, TL.ChatBase> Chats = new();

        private static ChatBase? _researchChannel = null;
        private static DialogFilter? _researchDialogFilter;

        public static async Task InitializeAsync(string groupName)
        {
            WTelegram.Helpers.Log = (lvl, str) => { };

            Me = await Client.LoginUserIfNeeded();

            var dialogs = await Client.Messages_GetAllDialogs(); // dialogs = groups/channels/users
            dialogs.CollectUsersChats(Users, Chats);

            _researchChannel = Chats.Values.SingleOrDefault(x => x.Title.Equals(groupName));

            var dialogFilters = await Client.Messages_GetDialogFilters();
            _researchDialogFilter = dialogFilters.FirstOrDefault(x => x is not null && x.Title.Equals(groupName)) as DialogFilter;

            //var contacts = await _client.Contacts_GetContacts();
            ////var user = contacts.users[1600841482];

            //var chat2 = Chats[1991392674];
            //var peers = solanaBots.include_peers.ToList();
            //peers.Add(chat2);
            //solanaBots.include_peers = peers.ToArray();
            //await _client.Messages_UpdateDialogFilter(solanaBots.ID, solanaBots);
            ////await _client.Contacts_AddContact(new InputUser(1600841482, 3496443273399859571), "овнер Test +1", string.Empty, string.Empty);
            //var chat1 = Chats[1833351991];
            //if (chat1 is TL.Channel channel1 && chat2 is TL.Channel channel2)
            //{
            //    var res = await _client.Account_UpdateNotifySettings(chat2, new InputPeerNotifySettings() { mute_until = int.MaxValue, flags = InputPeerNotifySettings.Flags.has_mute_until });
            //    var result1 = await _client.Account_GetNotifySettings(channel1);
            //    var result2 = await _client.Account_GetNotifySettings(channel2);

            //    //var chatInfo = await _client.Channels_GetFullChannel(channel);
            //    //var participants = await _client.Channels_GetParticipants(channel, new ChannelParticipantsAdmins());
            //    //var messages = await _client.Messages_GetHistory(chat);
            //    //if (chatInfo is not null && participants is not null)
            //    //{
            //    //    var adminList = participants.users.Where(x => !x.Value.IsBot).Select(x => (x.Value.first_name + x.Value.last_name, new InputUser(x.Value.id, x.Value.access_hash)));
            //    //    ChannelInformation channelInformation = new ChannelInformation(channel.id, channel.access_hash, chatInfo.full_chat.ParticipantsCount, adminList);

            //    //}
            //}

            //if (chat is TL.Channel channel)
            //{
            //    var part = await _client.Channels_GetParticipants(channel, new ChannelParticipantsContacts());
            //    int z = 1;
            //}

            //var result = await _client.Messages_ImportChatInvite("qIMVOF3EnH83MTUx");
            //var result = await _client.Contacts_ResolveUsername("eggwuhSOL");
            //if (result.Chat is Channel channel)
            //{
            //    //await _client.Channels_JoinChannel(channel);
            //    var part = await _client.Channels_GetParticipants(channel, new ChannelParticipantsAdmins());
            //    int z = 1;
            //}

            ChatListener.Initialize();
            CallTokenHelper.Initialize();
        }

        public static async Task<ChannelInformation?> JoinToChat(string? chatLink)
        {
            try
            {
                if (chatLink is null)
                    return null;

                var chat = await Client.AnalyzeInviteLink(chatLink, true, Chats);
                if (chat is TL.Channel channel)
                {
                    // выключаем уведомления
                    var settings = new InputPeerNotifySettings() { mute_until = int.MaxValue, flags = InputPeerNotifySettings.Flags.has_mute_until };
                    await Client.Account_UpdateNotifySettings(channel, settings);

                    if (_researchDialogFilter is not null)
                    {
                        var peers = _researchDialogFilter.include_peers.ToList();
                        peers.Add(channel);
                        _researchDialogFilter.include_peers = peers.ToArray();

                        await Client.Messages_UpdateDialogFilter(_researchDialogFilter.ID, _researchDialogFilter);
                    }

                    // собираем инфо про канал
                    var chatInfo = await Client.Channels_GetFullChannel(channel);
                    var admins = await Client.Channels_GetParticipants(channel, new ChannelParticipantsAdmins());
                    var contacts = await Client.Channels_GetParticipants(channel, new ChannelParticipantsContacts());
                    if (chatInfo is not null && admins is not null)
                    {
                        UserInformation? owner = null;
                        for (int i = 0; i < admins.participants.Length; i++)
                        {
                            if (admins.participants[i] is ChannelParticipantCreator creator)
                            {
                                var ownerUser = admins.users[creator.UserId];
                                owner = new UserInformation(ownerUser.first_name + ownerUser.last_name, new InputUser(ownerUser.id, ownerUser.access_hash));
                            }
                                
                        }

                        var ownerId = owner is not null ? owner.Id : -1;

                        var adminList = admins.users
                            .Where(x => !x.Value.IsBot && x.Value.ID != Me.ID && x.Value.ID != ownerId)
                            .Select(x => new UserInformation(x.Value.first_name + x.Value.last_name, new InputUser(x.Value.id, x.Value.access_hash)))
                            .ToList();
                        var contactList = contacts.users
                            .Where(x => !x.Value.IsBot && x.Value.ID != Me.ID && x.Value.ID != ownerId)
                            .Select(x => new UserInformation(x.Value.first_name + x.Value.last_name, new InputUser(x.Value.id, x.Value.access_hash)))
                            .Except(adminList)
                            .ToList();

                        return new ChannelInformation(channel.id, channel.access_hash, channel.Title, chatInfo.full_chat.ParticipantsCount, owner, adminList, contactList);

                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                return null;
            }
        }

        public static async Task RefreshChatInformation(ChannelInformation channel)
        {
            try
            {
                // собираем инфо про канал
                var chatInfo = await Client.Channels_GetFullChannel(channel);
                var admins = await Client.Channels_GetParticipants(channel, new ChannelParticipantsAdmins());
                var contacts = await Client.Channels_GetParticipants(channel, new ChannelParticipantsContacts());
                if (chatInfo is not null && admins is not null)
                {
                    channel.CurrentUserCount = chatInfo.full_chat.ParticipantsCount;

                    var ownerId = channel.Owner is not null ? channel.Owner.Id : -1;
                    channel.Admins = admins.users
                        .Where(x => !x.Value.IsBot && x.Value.ID != Me.ID && x.Value.ID != ownerId)
                        .Select(x => new UserInformation(x.Value.first_name + x.Value.last_name, new InputUser(x.Value.id, x.Value.access_hash)))
                        .ToList();
                    channel.Contacts = contacts.users
                        .Where(x => !x.Value.IsBot && x.Value.ID != Me.ID && x.Value.ID != ownerId)
                        .Select(x => new UserInformation(x.Value.first_name + x.Value.last_name, new InputUser(x.Value.id, x.Value.access_hash)))
                        .Except(channel.Admins)
                        .ToList();
                }
            }
            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }
        public static async Task LeaveChat(ChannelInformation channel)
        {
            try
            {
                // покинуть канал
                await Client.Channels_LeaveChannel(channel);
            }
            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }

        public static async Task AddChatAdminsToContacts(ChannelInformation channel, string description, bool flag)
        {
            try
            {
                if(channel.Owner is not null)
                {
                    channel.Owner.Name = OwnerParser.CreateNewOwnerName("овнер", channel.Owner.Name, flag);
                    await Client.Contacts_AddContact(channel.Owner, channel.Owner.Name, description, string.Empty);
                }

                foreach (var admin in channel.Admins)
                {
                    admin.Name = OwnerParser.CreateNewOwnerName("админ", admin.Name, flag);
                    await Client.Contacts_AddContact(admin, admin.Name, description, string.Empty);
                }
            }
            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }

        internal static async Task<MessageBase> ForwardMessageFromWatchingChat(Message messageEntity, InputChannel watchingChannel, int messageThreadId, bool pinMessage = false)
        {
            var result = await Client.Messages_ForwardMessages(watchingChannel, new[] { messageEntity.ID }, new[] { WTelegram.Helpers.RandomLong() }, _researchChannel, messageThreadId);
            var newMessage = result.UpdateList.OfType<TL.UpdateNewChannelMessage>().First();
            if (pinMessage)
                await Client.Messages_UpdatePinnedMessage(_researchChannel, newMessage.message.ID);

            return newMessage.message;
        }
    }
}
