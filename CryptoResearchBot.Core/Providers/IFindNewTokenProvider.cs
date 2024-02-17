using CryptoResearchBot.Core.Interfaces;
using Telegram.Bot;

namespace CryptoResearchBot.Core.Providers
{
    public interface IFindNewTokenProvider
    {
        Task InitializeAsync();

        Task<IEnumerable<ITokenData>> GetNewTokensAsync();
    }
}