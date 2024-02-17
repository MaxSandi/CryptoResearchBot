using CryptoResearchBot.Core.Common;
using CryptoResearchBot.Core.Extensions;
using CryptoResearchBot.Core.TelegramAPI;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace CryptoResearchBot.Core.Data
{
    public class WatchingTopic
    {
        public int Id => Data.Id;
        public int MainMessageId => Data.MainMessageId;

        public BaseWatchingTopicData Data { get; set; }

        public WatchingTopic(BaseWatchingTopicData watchingTopicData)
        {
            Data = watchingTopicData;
        }

        public async Task HandleMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is not null && update.CallbackQuery.Message is not null)
            {
                switch (update.CallbackQuery.Data)
                {
                    case TelegramCallbackData.RefreshInfo:
                        {
                            await RefreshInfoAsync(botClient, update, cancellationToken);
                            break;
                        }
                    case TelegramCallbackData.RemoveToken:
                        {
                            await RemoveTokenAsync(botClient, cancellationToken);
                            break;
                        }
                    case TelegramCallbackData.AddPlusOwner:
                        {
                            await TelegramApiProvider.AddChatAdminsToContacts(Data.ChannelInformation, string.Empty, true);
                            break;
                        }
                    case TelegramCallbackData.AddMinusOwner:
                        {
                            await TelegramApiProvider.AddChatAdminsToContacts(Data.ChannelInformation, string.Empty, false);
                            break;
                        }
                }
            }
            else if(update.Type == UpdateType.Message && update.Message is not null)
            {
                int z = 1;
            }


        }

        public void StartListen()
        {
            TL.InputChannel inputChahnnel = Data.ChannelInformation;
            ChatListener.AddWatchingChat(inputChahnnel.channel_id, ParseMessage);
        }

        public void StopListen()
        {
            TL.InputChannel inputChahnnel = Data.ChannelInformation;
            ChatListener.RemoveWatchingChat(inputChahnnel.channel_id);
        }

        private async Task RefreshInfoAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            await TelegramApiProvider.RefreshChatInformation(Data.ChannelInformation);

            try
            {
                await botClient.EditTokenMessage(Data.GetMainInformation(), update.CallbackQuery.Message.MessageId, cancellationToken);
            }
            catch (Exception e)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            }
        }

        private async Task RemoveTokenAsync(ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await TelegramApiProvider.LeaveChat(Data.ChannelInformation);

            await botClient.DeleteForumTopicAsync(TelegramConstants.ChatId, Id, cancellationToken);
        }

        private void ParseMessage(long chatId, TL.Message messageEntity)
        {
            Task.Run(async () =>
            {
                if(messageEntity.From is null || (Data.ChannelInformation.Owner is not null && Data.ChannelInformation.Owner.Id == messageEntity.From) || Data.ChannelInformation.Admins.Any(x => x.Id == messageEntity.From))
                {
                    //await _botClient.SendTextMessageAsync(TelegramConstants.ChatId, messageEntity.message, Id);
                    await TelegramApiProvider.ForwardMessageFromWatchingChat(messageEntity, Data.ChannelInformation, Id);
                }
            });
        }
    }
}
