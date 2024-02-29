using CryptoResearchBot.Core.Data;
using CryptoResearchBot.Core.Providers;
using CryptoResearchBot.SOL.Data;
using CryptoResearchBot.SOL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoResearchBot.SOL.Providers
{
    internal class SolTokenProvider : ITokenProvider
    {
        public async Task<BaseTokenData?> GetTokenDataFromContractAsync(string contractId)
        {
            var solTokenInfo = new SolTokenInfo() { Id = contractId };
            solTokenInfo.Name = await SolanaRPCHelper.GetTokenNameAsync(solTokenInfo.Id) ?? string.Empty;

            var ownerId = await SolanaRPCHelper.GetOwnerAsync(solTokenInfo.Id) ?? string.Empty;
            return new SolTokenData(solTokenInfo, ownerId);
        }
    }
}
