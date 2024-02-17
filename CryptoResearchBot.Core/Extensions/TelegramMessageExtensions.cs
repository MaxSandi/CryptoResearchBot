using CryptoResearchBot.Core.Common;
using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoResearchBot.Core.Extensions
{
    public static class TelegramMessageExtensions
    {
        public static InlineKeyboardMarkup NewFindedTokenKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("❌", TelegramCallbackData.DeleteTokenCallbackData)
                },
            });

        public static InlineKeyboardMarkup WatchingTokenKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("🔄", TelegramCallbackData.RefreshInfo),
                    InlineKeyboardButton.WithCallbackData("❌", TelegramCallbackData.RemoveToken)
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("🚀", TelegramCallbackData.AddPlusOwner),
                    InlineKeyboardButton.WithCallbackData("💩", TelegramCallbackData.AddMinusOwner)
                }
            });

        public static async Task<Telegram.Bot.Types.Message> SendTokenMessage(this ITelegramBotClient telegramBot, string text, int messageThreadId, CancellationToken cancellationToken)
        {
            return await telegramBot.SendTextMessageAsync(
                chatId: TelegramConstants.ChatId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                text: text, 
                disableWebPagePreview: true,
                messageThreadId: messageThreadId,
                cancellationToken: cancellationToken,
                replyMarkup: WatchingTokenKeyboard);
        }



        public static async Task<Telegram.Bot.Types.Message> EditTokenMessage(this ITelegramBotClient telegramBot, string text, int messageId, CancellationToken cancellationToken)
        {
            return await telegramBot.EditMessageTextAsync(
                    chatId: TelegramConstants.ChatId,
                    messageId: messageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                    text: text,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken,
                    replyMarkup: WatchingTokenKeyboard);
        }
    }
}
