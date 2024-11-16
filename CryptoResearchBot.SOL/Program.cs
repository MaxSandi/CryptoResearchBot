using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Network;
using CryptoResearchBot.Core.Providers;
using CryptoResearchBot.Core.TelegramAPI;
using CryptoResearchBot.Core.Worker;
using CryptoResearchBot.SOL;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

internal class Program
{
    private static IResearchProvider ResearchProvider = new SolResearchProvider();
    private static TelegramBotClient researchBot = new TelegramBotClient(Environment.GetEnvironmentVariable("api_bot"));

    public static async Task Main(string[] args)
    {
        await TelegramApiProvider.InitializeAsync(ResearchProvider.GroupName);

        using CancellationTokenSource cts = new();
        researchBot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            cancellationToken: cts.Token
        );

        var researchTokenWorker = new ResearchTokenWorker(ResearchProvider.FindNewTokenProvider);
        researchTokenWorker.NewTokensFinded += ResearchTokenWorker_NewTokensFinded;
        var researchTask = researchTokenWorker.StartResearch(cts.Token);


        // загружаем наблюдаемые токены
        ResearchProvider.LoadWatchingTopics();

        var result = Console.ReadLine();
        while (result is not null && result.ToLower().Contains("save"))
        {
            ResearchProvider.SaveWatchingTopics();

            result = Console.ReadLine();
        }

        ResearchProvider.SaveWatchingTopics();
        cts.Cancel();
        await researchTask;
    }

    private static void ResearchTokenWorker_NewTokensFinded(object? sender, IEnumerable<ITokenData> tokens)
    {
        Task.Run(async () =>
        {
            await ResearchProvider.HandleNewTokens(researchBot, tokens);
        });
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