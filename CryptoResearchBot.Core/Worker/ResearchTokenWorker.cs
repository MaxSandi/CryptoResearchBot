
using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Providers;
using Telegram.Bot;

namespace CryptoResearchBot.Core.Worker
{
    public class ResearchTokenWorker
    {
        private IFindNewTokenProvider _findNewTokenProvider;

        private const int DelayTime = 1000;

        public event EventHandler<IEnumerable<ITokenData>>? NewTokensFinded;

        public ResearchTokenWorker(IFindNewTokenProvider findNewTokenProvider)
        {
            _findNewTokenProvider = findNewTokenProvider;
        }

        public Task StartResearch(CancellationToken cancellationToken)
        {
            _findNewTokenProvider.InitializeAsync().Wait();

            return Task.Run(() => ResearchTask(cancellationToken), cancellationToken);
        }

        private async Task ResearchTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // find token
                    var newTokens = await _findNewTokenProvider.GetNewTokensAsync();
                    if (newTokens.Any())
                        NewTokensFinded?.Invoke(this, newTokens);

                    await Task.Delay(DelayTime, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // do nothing
                    Console.WriteLine("Cancel ResearchTokenWorker");
                }
                catch (Exception e)
                {
                    throw;
                }

            }
        }
    }
}
