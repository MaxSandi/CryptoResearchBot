using CryptoResearchBot.Core.Common;
using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.SOL.Data;
using CryptoResearchBot.SOL.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoResearchBot.SOL.Extensions
{
    internal static class TelegramMessageExtensions
    {
        public static InlineKeyboardMarkup NewFindedTokenKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("❌", TelegramCallbackData.DeleteTokenCallbackData)
                },
            });

        public static Task<Telegram.Bot.Types.Message> NewTokenFindMessage(this ITelegramBotClient telegramBot, long chatId, SolTokenData token)
        {
            var tokenInformation = token.GetInformation();
            var messageThreadId = (int)TopicType.NewTokens;

            return telegramBot.SendTextMessageAsync(
                chatId: chatId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                text: tokenInformation,
                messageThreadId: messageThreadId,
                disableWebPagePreview: true,
                replyMarkup: NewFindedTokenKeyboard);
        }

        public static async Task<Telegram.Bot.Types.Message> EditNewTokenMessage(this ITelegramBotClient telegramBot, long chatId, string text, int messageId, CancellationToken cancellationToken)
        {
            return await telegramBot.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: messageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                    text: text,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken,
                    replyMarkup: NewFindedTokenKeyboard);
        }
    }
}
