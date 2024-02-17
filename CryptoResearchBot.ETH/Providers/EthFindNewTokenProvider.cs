using CryptoResearchBot.Core.Interfaces;
using CryptoResearchBot.Core.Providers;

namespace CryptoResearchBot.ETH.Providers
{
    internal class EthFindNewTokenProvider : IFindNewTokenProvider
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public Task<IEnumerable<ITokenData>> GetNewTokensAsync()
        {
            throw new NotImplementedException();
        }
    }
}
