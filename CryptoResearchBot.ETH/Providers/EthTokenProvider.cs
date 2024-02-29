using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Providers;
using CryptoResearchBot.ETH.API;
using CryptoResearchBot.ETH.Data;

namespace CryptoResearchBot.ETH.Providers
{
    internal class EthTokenProvider : ITokenProvider
    {
        public async Task<BaseTokenData?> GetTokenDataFromContractAsync(string contractId)
        {
            var tokenInfo = await DexscreenerAPI.GetTokenInformationAsync(contractId);
            if (tokenInfo is null)
                return null;

            //TODO: научиться считывать владельца контракта
            return new EthTokenData(contractId, tokenInfo.baseToken.name, string.Empty);
        }
    }
}
