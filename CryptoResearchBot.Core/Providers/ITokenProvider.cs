using CryptoResearchBot.Core.Data;

namespace CryptoResearchBot.Core.Providers
{
    public interface ITokenProvider
    {
        Task<BaseTokenData?> GetTokenDataFromContractAsync(string contractId);
    }
}