using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Network;
using CryptoResearchBot.Core.TelegramAPI;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

internal class Program
{
    private static IResearchProvider ResearchProvider = ResearchProviderFactory.GetResearchProvider(-1002084456610);

    public static async Task Main(string[] args)
    {
        using CancellationTokenSource cts = new();
        TelegramBotClient researchBot = new TelegramBotClient(Environment.GetEnvironmentVariable("api_bot"));
        researchBot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            cancellationToken: cts.Token
        );

        await TelegramApiProvider.InitializeAsync(ResearchProvider.GroupName);

        // загружаем наблюдаемые токены
        ResearchProvider.LoadWatchingTopics();

        var result = Console.ReadLine();
        while (result is not null && result.ToLower().Contains("save"))
        {
            ResearchProvider.SaveWatchingTopics();

            result = Console.ReadLine();
        }

        cts.Cancel();
        ResearchProvider.SaveWatchingTopics();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            await ResearchProvider.HandleMessageAsync(botClient, update, cancellationToken);
        }
        catch (Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
        }
    }

    private static Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}